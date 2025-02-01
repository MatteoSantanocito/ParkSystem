// middleware.go
//
// Questo file definisce un middleware per la verifica del token JWT. Il middleware:
// 1. Legge l'header "Authorization" della richiesta, aspettandosi il formato "Bearer <token>".
// 2. Verifica e decodifica il token usando una chiave segreta.
// 3. Estrae il claim "user_id" (che identifica l'utente) e lo inserisce nel context della richiesta.
// 4. In caso di token mancante o non valido, risponde con un errore HTTP 401 Unauthorized.
// In questo modo, gli handler protetti potranno accedere all'ID utente tramite r.Context().

package middleware

import (
	"context"
	"fmt"
	"net/http"
	"strings"

	"github.com/golang-jwt/jwt/v4"
)

// JWTMiddleware è un middleware che verifica il token JWT presente nell'header "Authorization".
// Se il token è valido, estrae il claim "user_id" e lo inserisce nel context della richiesta.
// Il prossimo handler potrà così accedere a questo valore tramite r.Context().Value("userID").
func JWTMiddleware(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		// 1. Recupera l'header "Authorization" dalla richiesta.
		authHeader := r.Header.Get("Authorization")
		if authHeader == "" {
			// Se l'header non è presente, risponde con 401 Unauthorized.
			http.Error(w, "Missing Authorization header", http.StatusUnauthorized)
			return
		}

		// 2. Il formato atteso è "Bearer <token>".
		parts := strings.Split(authHeader, " ")
		if len(parts) != 2 || parts[0] != "Bearer" {
			// Se il formato non è corretto, risponde con un errore.
			http.Error(w, "Invalid Authorization header format", http.StatusUnauthorized)
			return
		}
		// parts[1] contiene il token JWT.
		tokenStr := parts[1]

		// 3. Parsing e verifica del token.
		// jwt.Parse richiede una funzione callback che fornisce la chiave segreta per la verifica.
		token, err := jwt.Parse(tokenStr, func(token *jwt.Token) (interface{}, error) {
			// Verifica che il metodo di firma sia HMAC (o il metodo atteso).
			if _, ok := token.Method.(*jwt.SigningMethodHMAC); !ok {
				return nil, fmt.Errorf("unexpected signing method: %v", token.Header["alg"])
			}
			// Definisce la chiave segreta per la firma del token.
			// In produzione, questa chiave non dovrebbe essere hardcoded ma recuperata da una configurazione sicura.
			secret := []byte("mysecretkey")
			return secret, nil
		})

		// Se c'è un errore durante il parsing o il token non è valido, ritorna 401.
		if err != nil || !token.Valid {
			http.Error(w, "Invalid token", http.StatusUnauthorized)
			return
		}

		// 4. Estrae i claims (informazioni) contenuti nel token.
		claims, ok := token.Claims.(jwt.MapClaims)
		if !ok {
			http.Error(w, "Invalid token claims", http.StatusUnauthorized)
			return
		}

		// 5. Estrae l'ID utente dal claim "user_id".
		// Spesso i valori numerici nei claims vengono trattati come float64.
		userIDFloat, ok := claims["user_id"].(float64)
		if !ok {
			http.Error(w, "User ID not found in token", http.StatusUnauthorized)
			return
		}
		userID := int(userIDFloat)

		// 6. Inserisce l'ID utente nel context della richiesta.
		// In questo modo, gli handler successivi possono recuperarlo usando r.Context().Value("userID").
		ctx := context.WithValue(r.Context(), "userID", userID)

		// 7. Passa la richiesta al prossimo handler, utilizzando il context aggiornato.
		next.ServeHTTP(w, r.WithContext(ctx))
	})
}
