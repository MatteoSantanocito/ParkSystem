public class Friendship
{
    public int id_richiesta { get; set; }
    public int id_utente_mittente { get; set; }
    public int id_utente_destinatario { get; set; }
    public string stato_richiesta { get; set; }
    public DateTime data_richiesta { get; set; }
}

public class FriendshipListResponse
{
    public List<Friendship> accepted { get; set; }
    public List<Friendship> pending { get; set; }
}
