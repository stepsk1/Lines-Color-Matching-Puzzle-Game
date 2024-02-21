create database linije
go
use linije
go
create table user_score(
	id int not null primary key identity(1,1),
	time int not null,
	score int not null
	)
go
INSERT INTO user_score (time, score) VALUES (25, 12);
go
INSERT INTO user_score (time, score) VALUES (36, 15);
go
INSERT INTO user_score (time, score) VALUES (6, 5);

select max(score) as best_score from user_score
select* from user_score
SELECT id, time, score FROM user_score WHERE score = (SELECT MAX(score) FROM user_score);