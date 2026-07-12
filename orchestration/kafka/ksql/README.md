# Kafka Streams (ksqlDB) Scripts

This directory contains ksqlDB scripts for stream processing on Kafka topics.

## Usage

1. **Access ksqlDB CLI:**
   ```bash
   docker exec -it ksql-cli ksql http://ksql-server:8088
   ```

2. **Create a stream from a Kafka topic:**
   ```sql
   CREATE STREAM messages_stream (
     id VARCHAR,
     content VARCHAR,
     routingKey VARCHAR,
     sender VARCHAR,
     timestamp BIGINT
   ) WITH (
     KAFKA_TOPIC='messages',
     VALUE_FORMAT='JSON'
   );
   ```

3. **Create a table for aggregations:**
   ```sql
   CREATE TABLE message_counts AS
   SELECT routingKey, COUNT(*) as count
   FROM messages_stream
   GROUP BY routingKey;
   ```

4. **Run persistent queries:**
   ```sql
   CREATE STREAM filtered_messages AS
   SELECT * FROM messages_stream
   WHERE content LIKE '%important%'.
   ```

## Access Points

- **ksqlDB Server:** http://localhost:8088
- **Schema Registry:** http://localhost:8085
- **Kafka UI:** http://localhost:8081

## Configuration

The ksqlDB server is configured to:
- Connect to Kafka at `kafka:9092`
- Use Schema Registry at `http://schema-registry:8081`
- Ignore hidden topics (starting with `_`)
