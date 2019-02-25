-- Table: public.transfer
-- DROP TABLE public.transfer;
CREATE TABLE public.transfer
(
	"timestamp" timestamp without time zone NOT NULL,
	
	operation_type smallint NOT NULL,
	 
	"from" character varying(16) COLLATE pg_catalog."default",
    "to" character varying(16) COLLATE pg_catalog."default",
    quantity bigint NOT NULL,
    asset_num integer NOT NULL,
	memo text

) PARTITION BY RANGE ("timestamp")
WITH (OIDS = FALSE)
TABLESPACE pg_default;
ALTER TABLE public.transfer OWNER to steem_data_light_user;          
