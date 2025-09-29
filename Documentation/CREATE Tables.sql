CREATE TABLE Country (
    id   SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL
);

CREATE TABLE Customer (
    id              SERIAL PRIMARY KEY,
    first_name      VARCHAR(255),
    last_name       VARCHAR(255),
    street          VARCHAR(255),
    zip             VARCHAR(20),
    place           VARCHAR(255),
    id_ct           INT,
    username        VARCHAR(255) UNIQUE,
    pw_hash         VARCHAR(255),
    user_created    TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE Deal (
    id           SERIAL PRIMARY KEY,
    total        FLOAT,
    last_update  TIMESTAMP,
    done         BOOLEAN
);

CREATE TABLE Shopping_Cart (
    id           SERIAL PRIMARY KEY,
    price        FLOAT,
    date_added   TIMESTAMP,
    id_dl		  INT,
    id_cs        INT,
    id_it        INT
);

CREATE TABLE Items (
    id           SERIAL PRIMARY KEY,
    name         VARCHAR(255),
    price        FLOAT,
    description  TEXT,
    id_ic        INT
);

CREATE TABLE Item_Category (
    id     SERIAL PRIMARY KEY,
    name   VARCHAR(255)
);