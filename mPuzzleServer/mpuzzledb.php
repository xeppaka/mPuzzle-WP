<?php

class DBNotConnectedException extends Exception { }
class DBQueryException extends Exception { }

class UserResult
{
	public $timeeasy;
	public $timemedium;
	public $timehard;
	public $timeexpert;
	public $moveseasy;
	public $movesmedium;
	public $moveshard;
	public $movesexpert;
}

class mPuzzleDB
{
	private $link;
	//private static $CREATE_DB_SQL = 'create database mpuzzle';
	private static $CREATE_TABLE_USERS_SQL = 'create table users (id int not null auto_increment, username varchar(50), primary key(id))';
	private static $CREATE_TABLE_RESULTS_SQL = 'create table results (userid int not null, puzzleid int not null, moveseasy int, movesmedium int, moveshard int, movesexpert int,
                      timeeasy int, timemedium int, timehard int, timeexpert int, primary key(userid, puzzleid), foreign key (userid) references users(id) on delete cascade)';
	private static $DROP_TABLE_USERS_SQL = 'drop table users';
	private static $DROP_TABLE_RESULTS_SQL = 'drop table results';

	function __construct() {
		$this->link = null;
	}
	
	public function connect() {
		$this->link = mysql_connect('xappmobi1.ipagemysql.com', 'mpuzzle', 'Enigma123');
		if ($this->link == null)
			return false;
		else
			return true;
	}
	
	public function disconnect() {
		mysql_close();
		$this->link = null;
	}
	
	// create mpuzzle tables
	public function create() {
		if ($this->link == null)
			throw new DBNotConnectedException();
		// if (!mysql_query(self::$CREATE_DB_SQL))
			// throw new DBQueryException();
		if (!mysql_select_db('mpuzzle'))
			throw new DBQueryException();
		if (!mysql_query(self::$CREATE_TABLE_USERS_SQL))
			throw new DBQueryException();
		if (!mysql_query(self::$CREATE_TABLE_RESULTS_SQL))
			throw new DBQueryException();
	}
	
	// drop all tables
	public function drop() {
		if ($this->link == null)
			throw new DBNotConnectedException();
		// if (!mysql_query(self::$CREATE_DB_SQL))
			// throw new DBQueryException();
		if (!mysql_select_db('mpuzzle'))
			throw new DBQueryException();
		if (!mysql_query(self::$DROP_TABLE_USERS_SQL))
			throw new DBQueryException();
		if (!mysql_query(self::$DROP_TABLE_RESULTS_SQL))
			throw new DBQueryException();
	}
	
	// checks if user exist in database
	// return : TRUE - if exist, FALSE - in other case
	public function userExist($username) {
		if ($this->link == null)
			throw new DBNotConnectedException();
		if (!mysql_select_db('mpuzzle'))
			throw new DBQueryException();
		
		$query = sprintf('select count(id) as idcount from users where username = \'%s\'', $username);
		$res = mysql_query($query);
		if (!$res)
			throw new DBQueryException();
		$row = mysql_fetch_assoc($res);
		mysql_free_result($res);
		if ($row['idcount'] >= 1)
			return true;
		else
			return false;
	}
	
	// create new user
	// return : -1 - user already exist, >= 0 - id, user created
	public function createUser($username) {
		if ($this->link == null)
			throw new DBNotConnectedException();
		if (!mysql_select_db('mpuzzle'))
			throw new DBQueryException();
		
		if ($this->userExist($username)) {
			return -1;
		}
		
		$query = sprintf('insert into users (username) values (\'%s\')', $username);
		if (!mysql_query($query))
			throw new DBQueryException();
		
		return mysql_insert_id();
	}

	// change username
	// return : 0 - OK, 1 - new username already exists, 2 - oldusername doesn't exist, 3 - userid and oldusername doesn't correspond each other
	public function changeUsername($userid, $oldusername, $newusername)
	{
		if ($this->link == null)
			throw new DBNotConnectedException();
		if (!mysql_select_db('mpuzzle'))
			throw new DBQueryException();
		if ($this->userExist($newusername)) {
			return 1;
		}
		if (!$this->userExist($oldusername)) {
			return 2;
		}
		$dbusername = $this->getUsername($userid);
		if (strcmp($dbusername, $oldusername) != 0)
			return 3;
		$query = sprintf('update users set username = \'%s\' where id=%d', $newusername, $userid);
		if (!mysql_query($query))
			throw new DBQueryException();
		return 0;
	}
	
	// get username from userid
	// return : not null - string username, null - username doesn't exist
	public function getUsername($userid) {
		if ($this->link == null)
			throw new DBNotConnectedException();
		if (!mysql_select_db('mpuzzle'))
			throw new DBQueryException();
				
		$query = sprintf('select username from users where id = %d', $userid);
		$res = mysql_query($query);
		if (!$res)
			throw new DBQueryException();
			
		if (mysql_num_rows($res) <= 0)
			return null;
			
		$row = mysql_fetch_assoc($res);
		mysql_free_result($res);
		
		return $row['username'];
	}
	
	// updates or creates result in the results table
	// $diff : 0 - easy, 1 - medium, 2 - hard, 3 - expert
	public function updateOrCreateResult($userid, $puzzleid, $time, $moves, $diff) {
		if ($this->link == null)
			throw new DBNotConnectedException();
		if (!mysql_select_db('mpuzzle'))
			throw new DBQueryException();

		$updtime = '';
		$updmoves = '';
		switch ($diff) {
			case 0:
				$updtime = 'timeeasy';
				$updmoves = 'moveseasy';
			break;
			case 1:
				$updtime = 'timemedium';
				$updmoves = 'movesmedium';
			break;
			case 2:
				$updtime = 'timehard';
				$updmoves = 'moveshard';
			break;
			case 3:
				$updtime = 'timeexpert';
				$updmoves = 'movesexpert';
			break;
		}
			
		$query = sprintf('select %s, %s from results where userid = %d and puzzleid = %d', $updtime, $updmoves, $userid, $puzzleid);
		$res = mysql_query($query);
		if (!$res)
			throw new DBQueryException();
		$rows = mysql_num_rows($res);
		if ($rows > 0)
			$row = mysql_fetch_assoc($res);
		mysql_free_result($res);
		
		if ($rows > 0) {
			// update here if got better result
			$timedb = $row[$updtime];
			$movesdb = $row[$updmoves];
			// if got better result update it
			if (($moves > 0 && $time > 0) && (($movesdb == 0 && $timedb == 0) || $moves < $movesdb || ($moves == $movesdb && $time < $timedb))) {
				$query = sprintf('update results set %s=%d, %s=%d where userid=%d and puzzleid=%d', $updtime, $time, $updmoves, $moves, $userid, $puzzleid);
				if (!mysql_query($query))
					throw new DBQueryException();
				$timedb = $time;
				$movesdb = $moves;
			}
		} else {
			// insert new result here
			$timeeasy = 0;$moveseasy = 0;
			$timemedium = 0;$movesmedium = 0;
			$timehard = 0;$moveshard = 0;
			$timeexpert = 0;$movesexpert = 0;
			
			switch ($diff) {
				case 0:
					$timeeasy = $time;
					$moveseasy = $moves;
				break;
				case 1:
					$timemedium = $time;
					$movesmedium = $moves;
				break;
				case 2:
					$timehard = $time;
					$moveshard = $moves;
				break;
				case 3:
					$timeexpert = $time;
					$movesexpert = $moves;
				break;
			}
			
			$query = sprintf('insert into results values (%d, %d, %d, %d, %d, %d, %d, %d, %d, %d)', $userid, $puzzleid, $moveseasy, $movesmedium, $moveshard, $movesexpert,
							$timeeasy, $timemedium, $timehard, $timeexpert);
			if (!mysql_query($query))
				throw new DBQueryException();
			
			$movesdb = $moves;
			$timedb = $time;
		}
		
		// get best result here
		$query = sprintf('select %s, %s from results where puzzleid = %d and %s <> 0 and %s <> 0 order by %s, %s', $updmoves, $updtime, $puzzleid, $updmoves, $updtime, $updmoves, $updtime);
		$res = mysql_query($query);
		if (!$res)
			throw new DBQueryException();
		$rows = mysql_num_rows($res);
		
		$timeb = 0;$movesb = 0;
		if ($rows > 0) {
			$row = mysql_fetch_assoc($res);
			$timeb = $row[$updtime];
			$movesb = $row[$updmoves];
		}
		mysql_free_result($res);
		
		$ret = array();
		$ret['timeb'] = $timeb;
		$ret['movesb'] = $movesb;
		$ret['userlist'] = array();
		$ret['moveslist'] = array();
		$ret['timelist'] = array();
		$ret['placelist'] = array();
		
		// 	get user place here (current result is used, not best)
		if ($time > 0 && $moves > 0) {
			$query = sprintf('select distinct %s, %s from results where puzzleid = %d and %s <> 0 and %s <> 0 and ((%s < %d) or (%s = %d and %s < %d))',
							 $updmoves, $updtime, $puzzleid, $updtime, $updmoves, $updmoves, $moves, $updmoves, $moves, $updtime, $time);
			$res = mysql_query($query);
			if (!$res)
				throw new DBQueryException();
			$ret['place'] = mysql_num_rows($res) + 1;
		} else {
			$ret['place'] = 0;
		}
		
		// get user list
		if ($ret['place'] > 0) {
			// 	get user place here (best result is used, not current)
			$query = sprintf('select %s, %s from results where puzzleid = %d and %s <> 0 and %s <> 0 and ((%s < %d) or (%s = %d and %s < %d))',
					 $updtime, $updmoves, $puzzleid, $updmoves, $updtime, $updmoves, $movesdb, $updmoves, $movesdb, $updtime, $timedb);
			$res = mysql_query($query);
			if (!$res)
				throw new DBQueryException();
			$position = mysql_num_rows($res) + 1;
			mysql_free_result($res);
			if ($position < 10) {
				$query = sprintf('select username, %s, %s from results join users on userid=id where puzzleid = %d and userid = %d',
								 $updtime, $updmoves, $puzzleid, $userid);
				$res = mysql_query($query);
				if (!$res)
					throw new DBQueryException();

				$row = mysql_fetch_array($res);
				if (!$row)
					throw new DBQueryException();
				mysql_free_result($res);
				
				$ourusername = $row[0].'[[[[[[[[[[you]]]]]]]]]]';
				$ourtime = $row[1];
				$ourmoves = $row[2];
				
				if ($position < 3)
					$selcount = 5;
				else
					$selcount = $position + 2;
					
				$query = sprintf('select username, %s, %s, userid from results join users on userid=id where puzzleid = %d and userid <> %d and %s <> 0 and %s <> 0 order by %s, %s limit 0, %d',
								 $updtime, $updmoves, $puzzleid, $userid, $updmoves, $updtime, $updmoves, $updtime, $selcount);
				$res = mysql_query($query);
				if (!$res)
					throw new DBQueryException();
								 
				$i = 0;
				$userinserted = false;
				$curmoves = 0; $curtime = 0;
				$curplace = 0;
				while ($row = mysql_fetch_array($res)) {
					if (!$userinserted) {
						if ($ourmoves < $row[2] || ($ourmoves == $row[2] && $ourtime <= $row[1])) {
							$ret['userlist'][$i] = $ourusername;
							$ret['timelist'][$i] = $ourtime;
							$ret['moveslist'][$i] = $ourmoves;
							if ($ourmoves > $curmoves || ($ourmoves == $curmoves && $ourtime > $curtime)) {
								$curplace++;
								$curmoves = $ourmoves;
								$curtime = $ourtime;
							}
							$ret['placelist'][$i] = $curplace;
							
							$userinserted = true;
							$i++;
						}
					}
					$ret['userlist'][$i] = $row[0];
					$ret['timelist'][$i] = $row[1];
					$ret['moveslist'][$i] = $row[2];
					if ($row[2] > $curmoves || ($row[2] == $curmoves && $row[1] > $curtime)) {
						$curplace++;
						$curmoves = $row[2];
						$curtime = $row[1];
					}
					$ret['placelist'][$i] = $curplace;
					$i++;
				}
				mysql_free_result($res);
				if (!$userinserted) {
					$ret['userlist'][$i] = $ourusername;
					$ret['timelist'][$i] = $ourtime;
					$ret['moveslist'][$i] = $ourmoves;
					if ($ourmoves > $curmoves || ($ourmoves == $curmoves && $ourtime > $curtime)) {
						$curplace++;
					}
					$ret['placelist'][$i] = $curplace;
				}
			} else {
				$query = sprintf('select username, %s, %s from results join users on userid=id where puzzleid = %d and %s <> 0 and %s <> 0 order by %s, %s limit 0, 5',
								 $updtime, $updmoves, $puzzleid, $updtime, $updmoves, $updmoves, $updtime);
				$res = mysql_query($query);
				if (!$res)
					throw new DBQueryException();
				$i = 0;
				$curmoves = 0; $curtime = 0;
				$curplace = 0;
				while ($row = mysql_fetch_array($res)) {
					$ret['userlist'][$i] = $row[0];
					$ret['timelist'][$i] = $row[1];
					$ret['moveslist'][$i] = $row[2];
					if ($row[2] > $curmoves || ($row[2] == $curmoves && $row[1] > $curtime)) {
						$curplace++;
						$curmoves = $row[2];
						$curtime = $row[1];
					}
					$ret['placelist'][$i] = $curplace;
					$i++;
				}
				mysql_free_result($res);
				
				$query = sprintf('select username, %s, %s from results join users on userid=id where puzzleid = %d and userid = %d',
								 $updtime, $updmoves, $puzzleid, $userid);
				$res = mysql_query($query);
				if (!$res)
					throw new DBQueryException();
					
				$row = mysql_fetch_array($res);
				if (!$row)
					throw new DBQueryException();
					
				$ourusername = $row[0].'[[[[[[[[[[you]]]]]]]]]]';
				$ourtime = $row[1];
				$ourmoves = $row[2];
				mysql_free_result($res);				
				
				$query = sprintf('select username, %s, %s from results join users on userid=id where puzzleid = %d and userid <> %d and %s <> 0 and %s <> 0 order by %s, %s limit %d, 4',
								 $updtime, $updmoves, $puzzleid, $userid, $updmoves, $updtime, $updmoves, $updtime, $position - 3);
				$res = mysql_query($query);
				if (!$res)
					throw new DBQueryException();
					
				// get place for the first selected user
				$row = mysql_fetch_array($res);
				if (!$row)
					throw new DBQueryException();
				
				$query = sprintf('select distinct %s, %s from results where puzzleid = %d and %s <> 0 and %s <> 0 and ((%s < %d) or (%s = %d and %s < %d))',
						 $updtime, $updmoves, $puzzleid, $updmoves, $updtime, $updmoves, $row[2], $updmoves, $row[2], $updtime, $row[1]);
				$tres = mysql_query($query);
				if (!$tres)
					throw new DBQueryException();
				$tplace = mysql_num_rows($tres);
				mysql_free_result($tres);
				mysql_data_seek($res, 0);
				
				$i = 5;
				$ret['userlist'][$i] = '...';
				$ret['timelist'][$i] = 0;
				$ret['moveslist'][$i] = 0;
				$ret['placelist'][$i] = 0;
				$i++;
				
				$i = 6;
				$userinserted = false;
				$curmoves = 0; $curtime = 0;
				$curplace = $tplace;
				while ($row = mysql_fetch_array($res)) {
					if (!$userinserted) {
						if ($ourmoves < $row[2] || ($ourmoves == $row[2] && $ourtime <= $row[1])) {
							$ret['userlist'][$i] = $ourusername;
							$ret['timelist'][$i] = $ourtime;
							$ret['moveslist'][$i] = $ourmoves;
							if ($ourmoves > $curmoves || ($ourmoves == $curmoves && $ourtime > $curtime)) {
								$curplace++;
								$curmoves = $ourmoves;
								$curtime = $ourtime;
							}
							$ret['placelist'][$i] = $curplace;
							
							$userinserted = true;
							$i++;
						}
					}
					$ret['userlist'][$i] = $row[0];
					$ret['timelist'][$i] = $row[1];
					$ret['moveslist'][$i] = $row[2];
					
					if ($row[2] > $curmoves || ($row[2] == $curmoves && $row[1] > $curtime)) {
						$curplace++;
						$curmoves = $row[2];
						$curtime = $row[1];
					}
					$ret['placelist'][$i] = $curplace;
					$i++;
				}
				mysql_free_result($res);
				if (!$userinserted) {
					$ret['userlist'][$i] = $ourusername;
					$ret['timelist'][$i] = $ourtime;
					$ret['moveslist'][$i] = $ourmoves;
					if ($ourmoves > $curmoves || ($ourmoves == $curmoves && $ourtime > $curtime)) {
						$curplace++;
					}
					$ret['placelist'][$i] = $curplace;
				}
			}
		} else {
			$query = sprintf('select username, %s, %s from results join users on userid=id where puzzleid = %d and %s <> 0 and %s <> 0 order by %s, %s limit 0, 5',
							 $updtime, $updmoves, $puzzleid, $updtime, $updmoves, $updmoves, $updtime);
			$res = mysql_query($query);
			if (!$res)
				throw new DBQueryException();
			$i = 0;
			$curmoves = 0; $curtime = 0;
			$curplace = 0;
			while ($row = mysql_fetch_array($res)) {
				$ret['userlist'][$i] = $row[0];
				$ret['timelist'][$i] = $row[1];
				$ret['moveslist'][$i] = $row[2];
				if ($row[2] > $curmoves || ($row[2] == $curmoves && $row[1] > $curtime)) {
					$curplace++;
					$curmoves = $row[2];
					$curtime = $row[1];
				}
				$ret['placelist'][$i] = $curplace;
				$i++;
			}
			mysql_free_result($res);
		}
		
		return $ret;
	}
	
	// check if result exist
	private function resultExist($userid, $puzzleid) {
		$query = sprintf('select count(userid) as idcount from results where userid = %d and puzzleid = %d', $userid, $puzzleid);
		$res = mysql_query($query);
		if (!$res)
			throw new DBQueryException();
		$row = mysql_fetch_assoc($res);
		mysql_free_result($res);
		if ($row['idcount'] >= 1)
			return true;
		else
			return false;
	}
	
	// gets user place
	// difficulty : 0 - easy, 1 - medium, 2 - hard, 3 - expert
	public function getUserPlace($userid, $puzzleid, $difficulty) {
		$query = sprintf('select count(id) as idcount from users where userid = %d and guid = \'%s\'', $userid, $guid);
		$res = mysql_query($query);
		if (!$res)
			throw new DBQueryException();
		$row = mysql_fetch_assoc($res);
		mysql_free_result($res);
		if ($row['idcount'] == 1)
			return true;
		else
			return false;
	}
	
	// get user results
	private function getUserResults($userid, $puzzleid) {
		if ($this->link == null)
			throw new DBNotConnectedException();
		if (!mysql_select_db('mpuzzle'))
			throw new DBQueryException();
		
		$query = sprintf('select count(id) as idcount from users where username = \'%s\'', $username);
		$res = mysql_query($query);
		if (!$res)
			throw new DBQueryException();
		$row = mysql_fetch_assoc($res);
		mysql_free_result($res);
		if ($row['idcount'] >= 1)
			return true;
		else
			return false;
	}
	
	// update user results
	// difficulty : 0 - easy, 1 - medium, 2 - hard, 3 - expert
	// return : > 0 - Result updated, user place, -1 - guid and userid not correspond
	public function updateScore($userid, $guid, $puzzleid, $difficulty, $result) {
		if ($this->link == null)
			throw new DBNotConnectedException();
			
		if (!checkUserid($userid, $guid))
			return -1;

		if (!mysql_select_db('mpuzzle'))
			throw new DBQueryException();

		if (!resultExist($userid, $puzzleid)) {
			switch ($difficulty) {
			case 0:
				$query = sprintf("insert into results (userid, puzzleid, moveseasy, movesmedium, moveshard, movesexpert, timeeasy, timemedium, timehard, timeexpert) 
					  values (%d, %d, %d, %d, %d, %d, %d, %d, %d, %d)", $userid, $puzzleid, $result->moveseasy, 0, 0, 0,
					  $result->timeeasy, 0, 0, 0);
				break;
			case 1:
				$query = sprintf("insert into results (userid, puzzleid, moveseasy, movesmedium, moveshard, movesexpert, timeeasy, timemedium, timehard, timeexpert) 
					  values (%d, %d, %d, %d, %d, %d, %d, %d, %d, %d)", $userid, $puzzleid, 0, $result->movesmedium, 0, 0,
					  0, $result->timemedium, 0, 0);
				break;
			case 2:
				$query = sprintf("insert into results (userid, puzzleid, moveseasy, movesmedium, moveshard, movesexpert, timeeasy, timemedium, timehard, timeexpert) 
					  values (%d, %d, %d, %d, %d, %d, %d, %d, %d, %d)", $userid, $puzzleid, 0, 0, $result->moveshard, 0,
					  0, 0, $result->timehard, 0);
				break;
			case 3:
				$query = sprintf("insert into results (userid, puzzleid, moveseasy, movesmedium, moveshard, movesexpert, timeeasy, timemedium, timehard, timeexpert) 
					  values (%d, %d, %d, %d, %d, %d, %d, %d, %d, %d)", $userid, $puzzleid, 0, 0, 0, $result->movesexpert,
					  0, 0, 0, $result->timeexpert);
				break;
			}
			if (!mysql_query($query))
				throw new DBQueryException();
		} else {
			switch ($difficulty) {
			case 0:
			$query = sprintf("update results set moveseasy=%d, timeeasy=%d where userid=%d and puzzleid=%d", $result->moveseasy, $result->timeeasy, $userid, $puzzleid);
				break;
			case 1:
			$query = sprintf("update results set movesmedium=%d, timemedium=%d where userid=%d and puzzleid=%d", $result->movesmedium, $result->timemedium, $userid, $puzzleid);
				break;
			case 2:
			$query = sprintf("update results set moveshard=%d, timehard=%d where userid=%d and puzzleid=%d", $result->moveshard, $result->timehard, $userid, $puzzleid);
				break;
			case 3:
			$query = sprintf("update results set movesexpert=%d, timeexpert=%d where userid=%d and puzzleid=%d", $result->movesexpert, $result->timeexpert, $userid, $puzzleid);
				break;
			}
			if (!mysql_query($query))
				throw new DBQueryException();
		}

		return 0;
	}	
}
?>