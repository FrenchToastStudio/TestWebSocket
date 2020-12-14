DROP DATABASE IF EXISTS phrases;
CREATE DATABASE IF NOT EXISTS phrases;
use phrases;

CREATE TABLE IF NOT EXISTS `listePhrases` (
  `id` int primary key auto_increment,
  `phrase` varchar(255) DEFAULT NULL
);

insert into `listePhrases` (phrase) values ("Ohhhh, mon dieu j'aime _____!!!");
insert into `listePhrases` (phrase) values ("Mon grand-père disait toujours: _____.");
insert into `listePhrases` (phrase) values ("J'aime bien les ____.");
insert into `listePhrases` (phrase) values ("Avant de devenir président, je dois détruire toute évidence de mon implication avec _____.");
insert into `listePhrases` (phrase) values ("Quand je serai milliardaire, j'érigerai une statue de 15 mètres pour commémorer _____.");
insert into `listePhrases` (phrase) values ("What did I bring back from Mexico?");
insert into `listePhrases` (phrase) values ("C'est votre capitaine qui parle. Attachez vos ceintures de sécurité et préparez-vous");