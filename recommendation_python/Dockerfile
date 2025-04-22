FROM python:3.11-slim

WORKDIR /app

RUN pip install psycopg2-binary pandas

COPY app.py /app/app.py

ENV DB_HOST=db_host
ENV DB_PORT=5432
ENV DB_USER=parksys
ENV DB_PASSWORD=password_db
ENV DB_NAME=parksystem_db

CMD ["python", "app.py"]
