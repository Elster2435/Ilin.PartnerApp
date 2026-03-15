-- =============================================
-- СОЗДАНИЕ РОЛИ И БАЗЫ ДАННЫХ
-- Выполнять от имени пользователя postgres
-- =============================================

CREATE ROLE app LOGIN PASSWORD '123456789';

DROP DATABASE IF EXISTS ilin;

CREATE DATABASE ilin
    OWNER app
    ENCODING 'UTF8';