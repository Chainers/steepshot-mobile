-- Table: public.account
-- DROP TABLE public.account;
CREATE TABLE public.account
(
	"timestamp" timestamp without time zone NOT NULL,
	
	login character varying(16) COLLATE pg_catalog."default",
    json_data text

) 
WITH (OIDS = FALSE) TABLESPACE pg_default;
ALTER TABLE public.node_info OWNER to steem_data_light_user;        
