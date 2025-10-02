 ## DADOS DO MEU BANCO DE DADOS: 
 ## server=localhost
 ## uid=roots
 ## pwd=root
 ## database=minimal_api

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";

CREATE DATABASE IF NOT EXISTS `minimal_apitest` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
USE `minimal_apitest`;

CREATE TABLE `administradores` (
  `ID` int(11) NOT NULL,
  `Email` varchar(255) NOT NULL,
  `Senha` varchar(50) NOT NULL,
  `Perfil` varchar(10) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `veiculos` (
  `ID` int(11) NOT NULL,
  `Nome` varchar(150) NOT NULL,
  `Marca` varchar(100) NOT NULL,
  `Ano` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `__efmigrationshistory` (
  `MigrationId` varchar(150) NOT NULL,
  `ProductVersion` varchar(32) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

ALTER TABLE `administradores`
  ADD PRIMARY KEY (`ID`);

ALTER TABLE `veiculos`
  ADD PRIMARY KEY (`ID`);

ALTER TABLE `__efmigrationshistory`
  ADD PRIMARY KEY (`MigrationId`);

ALTER TABLE `administradores`
  MODIFY `ID` int(11) NOT NULL AUTO_INCREMENT;

ALTER TABLE `veiculos`
  MODIFY `ID` int(11) NOT NULL AUTO_INCREMENT;
COMMIT;
