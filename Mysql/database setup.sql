create database if not exists dancer;
# remember to change the default character in server to utf8, just write into my.cnf.
use dancer;
set sql_safe_updates=0; # else the trigger cannot work.
drop table if exists music;
create table music
(
	music_name varchar(255) not null,
    singer varchar(255) not null,
    other_singer varchar(255),
    song_id int not null auto_increment,
    weight double not null default 1,
    belong_to_list varchar(255) not null references lists.list_name,
    album varchar(255),
    publish_year int,
    add_in_date timestamp not null default current_timestamp,
    primary key(song_id),
    key(music_name, singer)
)default charset=utf8; 
drop table if exists listening;
create table listening
(
	music_name varchar(255)not null references music.music_name,
    singer varchar(255)not null references music.singer,
    listening_time timestamp not null default current_timestamp,
    where_to_listen varchar(16),
    primary key(music_name, singer, listening_time)
)default charset=utf8; 
drop table if exists lists;
create table lists
(
	list_name varchar(255) not null unique primary key
)default charset=utf8; 
drop trigger if exists trigger_listen_a_song;
delimiter | 
create trigger trigger_listen_a_song
after insert on listening for each row
begin
	declare weight_sum double;
    declare songs_sum double;
    select count(*)from music into songs_sum;
	update music set music.weight = 0.1 / songs_sum
    where music.music_name = new.music_name and music.singer = new.singer;
    select sum(music.weight)from music into weight_sum;
    update music set music.weight = music.weight/weight_sum where music.weight != 0;
end|
delimiter ;
drop procedure if exists get_current_song;
delimiter |
create procedure get_current_song
(out _music_name varchar(255), out _singer varchar(255))
begin
	declare rand double;
	declare weight_sum double default 0;
    declare i int;
    select rand = rand();
    select i = max(music.song_id) from music;
    outer_label:
    while i > 0 do
		set weight_sum = weight_sum +
			(select music.weight from music where music.song_id = i);
		if weight_sum > rand then
			select _music_name = music.music_name, _singer = music.singer
            from music where music.song_id = i;
            leave outer_label;
		end if;
		set i = i - 1;
    end while;
end|
delimiter ;










