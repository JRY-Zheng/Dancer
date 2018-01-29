use dancer;
drop table if exists dancer_user;
create table dancer_user
(
	user_name varchar(255) not null primary key
    /*user_password varchar(255) not null*/
)default charset=utf8; 
insert into dancer_user(user_name) values('root');
alter table music add user_name varchar(255) not null first;
alter table music add foreign key(user_name) references dancer_user(user_name);
alter table music drop key music_name;
alter table music add key(user_name, music_name, singer);
desc music;
alter table lists add user_name varchar(255) not null;
alter table lists add foreign key(user_name) references dancer_user(user_name);
alter table lists drop primary key;
alter table lists add primary key(list_name, user_name);
desc lists;
alter table listening add user_name varchar(255) not null;
alter table listening add foreign key(user_name) references dancer_user(user_name);
alter table listening drop primary key;
alter table listening add primary key(music_name, singer, listening_time, user_name);
desc listening;

