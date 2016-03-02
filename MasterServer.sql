-- phpMyAdmin SQL Dump
-- version 3.3.7
-- http://www.phpmyadmin.net

SET SQL_MODE="NO_AUTO_VALUE_ON_ZERO";

-- --------------------------------------------------------

--
-- Table structure for table `MasterServer`
--

CREATE TABLE IF NOT EXISTS `MasterServer` (
  `useNat` tinyint(1) NOT NULL,
  `gameType` varchar(256) NOT NULL,
  `gameName` varchar(256) NOT NULL,
  `connectedPlayers` mediumint(16) NOT NULL,
  `playerLimit` smallint(8) NOT NULL,
  `internalIp` varchar(64) NOT NULL,
  `internalPort` smallint(8) unsigned NOT NULL,
  `externalIP` varchar(64) NOT NULL,
  `externalPort` smallint(8) NOT NULL,
  `guid` varchar(255) NOT NULL,
  `passwordProtected` tinyint(1) NOT NULL,
  `comment` blob NOT NULL,
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `updated` int(64) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `gameName_3` (`gameName`),
  FULLTEXT KEY `gameName_2` (`gameName`)
) ENGINE=MyISAM  DEFAULT CHARSET=utf8 AUTO_INCREMENT=862 ;
