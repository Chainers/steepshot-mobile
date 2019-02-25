-------------------------------------------------------------------------------------------------------------------------------------
-- User: steem_data_light_user
-- DROP USER steem_data_light_user;
CREATE USER steem_data_light_user WITH
  LOGIN
  NOSUPERUSER
  INHERIT
  NOCREATEDB
  NOCREATEROLE
  NOREPLICATION;

alter user steem_data_light_user with encrypted password '123456789' <= set password;

-------------------------------------------------------------------------------------------------------------------------------------
-- Database: steem_data_light
-- DROP DATABASE steem_data_light;
CREATE DATABASE steem_data_light
    WITH 
    OWNER = steem_data_light_user
    ENCODING = 'UTF8'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1;

-------------------------------------------------------------------------------------------------------------------------------------
-- SEQUENCE: public.node_info_id_seq
-- DROP SEQUENCE public.node_info_id_seq;
CREATE SEQUENCE public.node_info_id_seq;
ALTER SEQUENCE public.node_info_id_seq OWNER TO steem_data_light_user;

-- Table: public.node_info
-- DROP TABLE public.node_info;
CREATE TABLE public.node_info
(
    id integer NOT NULL DEFAULT nextval('node_info_id_seq'::regclass),
    url text COLLATE pg_catalog."default",
    success_count integer NOT NULL,
    fail_count integer NOT NULL,
    elapsed_milliseconds integer NOT NULL,
    CONSTRAINT pk_node_info PRIMARY KEY (id)
)
WITH (OIDS = FALSE) TABLESPACE pg_default;
ALTER TABLE public.node_info OWNER to steem_data_light_user;

--INSERT INTO public.node_info(url, success_count, fail_count, elapsed_milliseconds) VALUES ('https://api.steemit.com', 1, 0, 300);

-------------------------------------------------------------------------------------------------------------------------------------
-- SEQUENCE: public.version_id_seq
-- DROP SEQUENCE public.version_id_seq;
CREATE SEQUENCE public.version_id_seq;
ALTER SEQUENCE public.version_id_seq OWNER TO steem_data_light_user;
-- Table: public.version
-- DROP TABLE public.version;
CREATE TABLE public.version
(
    id integer NOT NULL DEFAULT nextval('version_id_seq'::regclass),
    name text COLLATE pg_catalog."default",
    CONSTRAINT pk_version PRIMARY KEY (id)
)
WITH (OIDS = FALSE) TABLESPACE pg_default;
ALTER TABLE public.version OWNER to steem_data_light_user;

-------------------------------------------------------------------------------------------------------------------------------------
-- Table: public.service_state
-- DROP TABLE public.service_state;
CREATE TABLE public.service_state
(
	service_id integer NOT NULL,
    json text COLLATE pg_catalog."default",
    CONSTRAINT pk_service_state PRIMARY KEY (service_id)
)
WITH (OIDS = FALSE)
TABLESPACE pg_default;
ALTER TABLE public.service_state OWNER to steem_data_light_user;
INSERT INTO public.service_state(service_id, json) VALUES (1, '{"BlockId":1}');
-------------------------------------------------------------------------------------------------------------------------------------