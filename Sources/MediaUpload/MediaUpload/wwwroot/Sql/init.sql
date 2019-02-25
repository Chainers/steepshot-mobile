------------------------------------------------------------------------------------------
-- User: media_data_user
-- DROP USER media_data_user;
CREATE USER media_data_user WITH
  LOGIN
  NOSUPERUSER
  INHERIT
  NOCREATEDB
  NOCREATEROLE
  NOREPLICATION;

alter user media_data_user with encrypted password '123456789' <= set new password;

------------------------------------------------------------------------------------------
-- Database: media_data
-- DROP DATABASE media_data;
CREATE DATABASE media_data
    WITH 
    OWNER = media_data_user
    ENCODING = 'UTF8'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1;

------------------------------------------------------------------------------------------