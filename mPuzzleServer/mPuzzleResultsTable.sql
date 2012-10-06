use mpuzzle;
create table users(id int not null auto_increment, guid char(36), username varchar(50), primary key(id));
create table results (userid int not null, puzzleid int not null, moveseasy int, movesmedium int, moveshard int, movesexpert int,
                      timeeasy int, timemedium int, timehard int, timeexpert int, primary key(userid, puzzleid), foreign key (userid) references users(id) on delete cascade);
