<?php

require_once 'mpuzzledb.php';

/**
 * mPuzzle create DB service
 * @namespace mpuzzle
 * @uri /rest/create
 */
 
class mPuzzleCreateTablesResource extends Resource {
    
	private $db;
	
	function __construct($parameters) {
		parent::__construct($parameters);
		$this->db = new mPuzzleDB();
	}
	
    function post($request) {
        
        $response = new Response($request);
		
		$this->db->connect();
		try {
			$response->code = Response::OK;
			$this->db->create();
			$response->body = 'Tables created';
		} catch (Exception $e) {
			$response->body = 'Tables already exist';
		}
		$this->db->disconnect();
        
        return $response;
    }
}

/**
 * mPuzzle create DB service
 * @namespace mpuzzle
 * @uri /rest/drop
 */
 
class mPuzzleDropTablesResource extends Resource {
    
	private $db;
	
	function __construct($parameters) {
		parent::__construct($parameters);
		$this->db = new mPuzzleDB();
	}
	
    function post($request) {
        
        $response = new Response($request);
		
		$this->db->connect();
		try {
			$response->code = Response::OK;
			$this->db->drop();
			$response->body = 'Tables dropped';
		} catch (Exception $e) {
			$response->body = 'Tables aren\'t exist';
		}
		$this->db->disconnect();
        
        return $response;
    }
}

/**
 * mpuzzle REST services
 * @namespace mpuzzle
 * @uri /rest/user/{userid}
 */
 
class mPuzzleUserIdResource extends Resource {
    
	private $db;
	
	function __construct($parameters) {
		parent::__construct($parameters);
		$this->db = new mPuzzleDB();
	}
	
    function get($request, $userid) {
        
        $response = new Response($request);
		
		$this->db->connect();
		try {
			$username = $this->db->getUsername($userid);
			if ($username == null)
				$response->code = Response::NOTFOUND;
			else {
				$response->code = Response::OK;
				$response->body = $username;
			}
		} catch (Exception $e) { $response->code = Response::INTERNALSERVERERROR; }
		$this->db->disconnect();
		
        return $response;
    }
}

/**
 * mpuzzle REST services
 * @namespace mpuzzle
 * @uri /rest/user
 */
class mPuzzleUsersResource extends Resource {
    
	private $db;
	private $username;
	private $curvar;
	
	function __construct($parameters) {
		parent::__construct($parameters);
		$this->db = new mPuzzleDB();
	}
	
	function startTag($parser, $data) {
		switch (strtolower($data))
		{
			case 'username':
				$this->curvar = 1;
				break;
			default:
				$this->curvar = -1;
		}
	}
	
	function endTag($parser, $data) {
		$this->curvar = -1;
	}
	
	function content($parser, $data) {
		switch ($this->curvar)
		{
			case 1:
				$this->username = $data;
				break;
			default:
				break;
		}
	}
	
    function post($request) {
        $response = new Response($request);
		
		$xmlparser = xml_parser_create();
		xml_set_object($xmlparser, $this);
		xml_set_element_handler($xmlparser, 'startTag', 'endTag');
		xml_set_character_data_handler($xmlparser, 'content');
		if (!xml_parse($xmlparser, $request->data, true)) {
			$response->code = Response::BADREQUEST;
			$response->body = 'Couldn\'t parse XML';
			return $response;
		}
		xml_parser_free($xmlparser);
		
		if (strlen($this->username) <= 0) {
			$response->code = Response::BADREQUEST;
			$response->body = '';
		} else {
			$this->db->connect();
			try {
				$id = $this->db->createUser($this->username);
				$this->db->disconnect();
				$response->code = Response::CREATED;
				$response->body = '<UserCreateResponse><userid>'.$id.'</userid></UserCreateResponse>';
			} catch (Exception $e) {
				$response->code = Response::INTERNALSERVERERROR;
				$response->body = '';
			}
		}
        
        return $response;
    }
}

/**
 * mpuzzle REST services
 * @namespace mpuzzle
 * @uri /rest/user/{userid}
 */
class mPuzzleUsersIdResource extends Resource {
    
	private $db;
	private $oldusername;
	private $newusername;
	private $curvar;
	
	function __construct($parameters) {
		parent::__construct($parameters);
		$this->db = new mPuzzleDB();
	}
	
	function startTag($parser, $data) {
		switch (strtolower($data))
		{
			case 'oldusername':
				$this->curvar = 1;
				break;
			case 'newusername':
				$this->curvar = 2;
				break;
			default:
				$this->curvar = -1;
		}
	}
	
	function endTag($parser, $data) {
		$this->curvar = -1;
	}
	
	function content($parser, $data) {
		switch ($this->curvar)
		{
			case 1:
				$this->oldusername = $data;
				break;
			case 2:
				$this->newusername = $data;
				break;
			default:
				break;
		}
	}
	
    function post($request, $userid) {
        $response = new Response($request);
		
		$xmlparser = xml_parser_create();
		xml_set_object($xmlparser, $this);
		xml_set_element_handler($xmlparser, 'startTag', 'endTag');
		xml_set_character_data_handler($xmlparser, 'content');
		if (!xml_parse($xmlparser, $request->data, true)) {
			$response->code = Response::BADREQUEST;
			$response->body = 'Couldn\'t parse XML';
			return $response;
		}
		xml_parser_free($xmlparser);
		
		if (strlen($this->newusername) <= 0 || strlen($this->oldusername) <= 0) {
			$response->code = Response::BADREQUEST;
			$response->body = '';
		} else {
			$this->db->connect();
			try {
				$res = $this->db->changeUsername($userid, $this->oldusername, $this->newusername);
				$this->db->disconnect();
				$response->code = Response::OK;
				$response->body = '<UserChangeResponse><result>'.$res.'</result></UserChangeResponse>';
			} catch (Exception $e) {
				$response->code = Response::INTERNALSERVERERROR;
				$response->body = '';
			}
		}
        
        return $response;
    }
}

/**
 * mpuzzle REST services
 * @namespace mpuzzle
 * @uri /rest/user/{userid}/puzzle/{puzzleid}/{diff}
 */
class mPuzzleResultOneSyncResource extends Resource {
	private $db;
	private $time;
	private $moves;
	private $curvar;
	
	function __construct($parameters) {
		parent::__construct($parameters);
		$this->db = new mPuzzleDB();
	}
	
	function startTag($parser, $data) {
		switch (strtolower($data))
		{
			case 'time':
				$this->curvar = 1;
				break;
			case 'moves':
				$this->curvar = 2;
				break;
			default:
				$this->curvar = -1;
				break;
		}
	}
	
	function endTag($parser, $data) {
		$this->curvar = -1;
	}
	
	function content($parser, $data) {
		switch ($this->curvar)
		{
			case 1:
				$this->time = (int)$data;
				break;
			case 2:
				$this->moves = (int)$data;
				break;
		}
	}
	
	function post($request, $userid, $puzzleid, $diff) {
        $response = new Response($request);
		$xmlparser = xml_parser_create();
		xml_set_object($xmlparser, $this);
		xml_set_element_handler($xmlparser, 'startTag', 'endTag');
		xml_set_character_data_handler($xmlparser, 'content');
		if (!xml_parse($xmlparser, $request->data, true)) {
			$response->code = Response::BADREQUEST;
			$response->body = 'Couldn\'t parse XML';
			return $response;
		}
		xml_parser_free($xmlparser);
		
		$idiff = -1;
		switch (strtolower($diff)) {
			case 'easy':
				$idiff = 0;
				break;
			case 'medium':
				$idiff = 1;
				break;
			case 'hard':
				$idiff = 2;
				break;
			case 'expert':
				$idiff = 3;
				break;
		}
		
		try {
			if ($idiff >= 0) {
				$this->db->connect();
				$res = $this->db->updateOrCreateResult((int)$userid, $puzzleid, $this->time, $this->moves, $idiff);
				$this->db->disconnect();
				$xml = '<SyncOneResponse><besttime>'.$res['timeb'].'</besttime><bestmoves>'.$res['movesb'].'</bestmoves><place>'.$res[place].'</place><userlist>';
				$usercount = count($res['userlist']);
				for ($i = 0;$i < $usercount;$i++) {
					$xml .= '<user username=\''.$res['userlist'][$i].'\' place=\''.$res['placelist'][$i].'\'>';
					$xml .= '<time>'.$res['timelist'][$i].'</time>';
					$xml .= '<moves>'.$res['moveslist'][$i].'</moves></user>';
				}
				$xml .= '</userlist></SyncOneResponse>';
				$response->code = Response::OK;
				$response->body = $xml;
			} else {
				$response->code = Response::BADREQUEST;
				$response->body = '';
			}
		} catch (Exception $e) {
			$response->code = Response::INTERNALSERVERERROR;
			$response->body = '';
		}
		
		return $response;
	}
}

class PuzzleResult {
	public $timeeasy;
	public $timemedium;
	public $timehard;
	public $timeexpert;
	public $moveseasy;
	public $movesmedium;
	public $moveshard;
	public $movesexpert;
	public $puzzleid;
	
	function __construct() {
		$this->timeeasy = $this->timemedium = $this->timehard = $this->timeexpert = 0;
		$this->moveseasy = $this->movesmedium = $this->moveshard = $this->movesexpert = 0;
		$this->puzzleid = 0;
	}
};

/**
 * mpuzzle REST services
 * @namespace mpuzzle
 * @uri /rest/user/{userid}/puzzle
 */

class mPuzzleResultAllSyncResource extends Resource {
	private $db;
	private $puzzles;
	private $curvar;
	private $curindex;
	
	function __construct($parameters) {
		parent::__construct($parameters);
		$this->db = new mPuzzleDB();
		$puzzles = array();
		$curindex = -1;
	}
	
	function startTag($parser, $data, $attr) {
		switch (strtolower($data))
		{
			case 'puzzleresult':
				$this->curindex++;
				$this->puzzles[$this->curindex] = new PuzzleResult();
				$this->puzzles[$this->curindex]->puzzleid = (int)$attr['PUZZLEID'];
				$this->curvar = -1;
				break;
			case 'timeeasy':
				$this->curvar = 1;
				break;
			case 'timemedium':
				$this->curvar = 2;
				break;
			case 'timehard':
				$this->curvar = 3;
				break;
			case 'timeexpert':
				$this->curvar = 4;
				break;
			case 'moveseasy':
				$this->curvar = 5;
				break;
			case 'movesmedium':
				$this->curvar = 6;
				break;
			case 'moveshard':
				$this->curvar = 7;
				break;
			case 'movesexpert':
				$this->curvar = 8;
				break;
			default:
				$this->curvar = -1;
		}
	}
	
	function endTag($parser, $data) {
		$this->curvar = -1;
	}
	
	function content($parser, $data) {
		switch ($this->curvar)
		{
			case 1:
				$this->puzzles[$this->curindex]->timeeasy = (int)$data;
				break;
			case 2:
				$this->puzzles[$this->curindex]->timemedium = (int)$data;
				break;
			case 3:
				$this->puzzles[$this->curindex]->timehard = (int)$data;
				break;
			case 4:
				$this->puzzles[$this->curindex]->timeexpert = (int)$data;
				break;
			case 5:
				$this->puzzles[$this->curindex]->moveseasy = (int)$data;
				break;
			case 6:
				$this->puzzles[$this->curindex]->movesmedium = (int)$data;
				break;
			case 7:
				$this->puzzles[$this->curindex]->moveshard = (int)$data;
				break;
			case 8:
				$this->puzzles[$this->curindex]->movesexpert = (int)$data;
				break;
			default:
				break;
		}
	}
	
    function post($request, $userid) {
        
        $response = new Response($request);
		$xmlparser = xml_parser_create();
		xml_set_object($xmlparser, $this);
		xml_set_element_handler($xmlparser, 'startTag', 'endTag');
		xml_set_character_data_handler($xmlparser, 'content');
		if (!xml_parse($xmlparser, $request->data, true)) {
			$response->code = Response::BADREQUEST;
			$response->body = 'Couldn\'t parse XML';
			return $response;
		}
		xml_parser_free($xmlparser);

		
		try {
			$this->db->connect();
			$result = '<SyncAllResponse>';
			foreach ($this->puzzles as $puzzle) {
				$result .= '<PuzzleBestResult puzzleid="'.$puzzle->puzzleid.'">';
				$result .= '<timeeasy>';
				$res = $this->db->updateOrCreateResult((int)$userid, $puzzle->puzzleid, $puzzle->timeeasy, $puzzle->moveseasy, 0);
				if ($res) {
					$result .= $res['timeb'];
					$result .= '</timeeasy><moveseasy>';
					$result .= $res['movesb'];
					$result .= '</moveseasy><placeeasy>';
					$result .= $res['place'];
					$result .= '</placeeasy>';
					$usercount = count($res['userlist']);
					$result .= '<userlisteasy>';
					for ($i = 0;$i < $usercount;$i++) {
						$result .= '<user username=\''.$res['userlist'][$i].'\' place=\''.$res['placelist'][$i].'\'>';
						$result .= '<time>'.$res['timelist'][$i].'</time>';
						$result .= '<moves>'.$res['moveslist'][$i].'</moves></user>';
					}
					$result .= '</userlisteasy>';
				}
				$result .= '<timemedium>';
				$res = $this->db->updateOrCreateResult((int)$userid, $puzzle->puzzleid, $puzzle->timemedium, $puzzle->movesmedium, 1);
				if ($res) {
					$result .= $res['timeb'];
					$result .= '</timemedium><movesmedium>';
					$result .= $res['movesb'];
					$result .= '</movesmedium><placemedium>';
					$result .= $res['place'];
					$result .= '</placemedium>';
					$usercount = count($res['userlist']);
					$result .= '<userlistmedium>';
					for ($i = 0;$i < $usercount;$i++) {
						$result .= '<user username=\''.$res['userlist'][$i].'\' place=\''.$res['placelist'][$i].'\'>';
						$result .= '<time>'.$res['timelist'][$i].'</time>';
						$result .= '<moves>'.$res['moveslist'][$i].'</moves></user>';
					}
					$result .= '</userlistmedium>';
				}
				$result .= '<timehard>';
				$res = $this->db->updateOrCreateResult((int)$userid, $puzzle->puzzleid, $puzzle->timehard, $puzzle->moveshard, 2);
				if ($res) {
					$result .= $res['timeb'];
					$result .= '</timehard><moveshard>';
					$result .= $res['movesb'];
					$result .= '</moveshard><placehard>';
					$result .= $res['place'];
					$result .= '</placehard>';
					$usercount = count($res['userlist']);
					$result .= '<userlisthard>';
					for ($i = 0;$i < $usercount;$i++) {
						$result .= '<user username=\''.$res['userlist'][$i].'\' place=\''.$res['placelist'][$i].'\'>';
						$result .= '<time>'.$res['timelist'][$i].'</time>';
						$result .= '<moves>'.$res['moveslist'][$i].'</moves></user>';
					}
					$result .= '</userlisthard>';
				}
				$result .= '<timeexpert>';
				$res = $this->db->updateOrCreateResult((int)$userid, $puzzle->puzzleid, $puzzle->timeexpert, $puzzle->movesexpert, 3);
				if ($res) {
					$result .= $res['timeb'];
					$result .= '</timeexpert><movesexpert>';
					$result .= $res['movesb'];
					$result .= '</movesexpert><placeexpert>';
					$result .= $res['place'];
					$result .= '</placeexpert>';
					$usercount = count($res['userlist']);
					$result .= '<userlistexpert>';
					for ($i = 0;$i < $usercount;$i++) {
						$result .= '<user username=\''.$res['userlist'][$i].'\' place=\''.$res['placelist'][$i].'\'>';
						$result .= '<time>'.$res['timelist'][$i].'</time>';
						$result .= '<moves>'.$res['moveslist'][$i].'</moves></user>';
					}
					$result .= '</userlistexpert>';
				}
				$result .= '</PuzzleBestResult>';
			}
			$this->db->disconnect();
			$result .= '</SyncAllResponse>';
			$response->code = Response::OK;
			$response->body = $result;
		} catch (Exception $e) {
			$response->code = Response::INTERNALSERVERERROR;
			$response->body = '';
		}
		
        return $response;
    }
}

?>