package com.xeppaka.mpuzzle.entities;

import javax.jdo.annotations.IdGeneratorStrategy;
import javax.jdo.annotations.PersistenceCapable;
import javax.jdo.annotations.Persistent;
import javax.jdo.annotations.PrimaryKey;

import com.google.appengine.api.datastore.Key;

@PersistenceCapable
public class GameResult {
	@PrimaryKey
	@Persistent(valueStrategy = IdGeneratorStrategy.IDENTITY)
	private Key key;
	
	@Persistent
	private Integer puzzleid;
	
	@Persistent
	private Integer moveseasy;
	
	@Persistent
	private Integer movesmedium;
	
	@Persistent
	private Integer moveshard;
	
	@Persistent
	private Integer movesexpert;
	
	@Persistent
	private Integer timeeasy;
	
	@Persistent
	private Integer timemedium;
	
	@Persistent
	private Integer timehard;
	
	@Persistent
	private Integer timeexpert;

	public Integer getPuzzleid() {
		return puzzleid;
	}

	public void setPuzzleid(Integer puzzleid) {
		this.puzzleid = puzzleid;
	}

	public Integer getMoveseasy() {
		return moveseasy;
	}

	public void setMoveseasy(Integer moveseasy) {
		this.moveseasy = moveseasy;
	}

	public Integer getMovesmedium() {
		return movesmedium;
	}

	public void setMovesmedium(Integer movesmedium) {
		this.movesmedium = movesmedium;
	}

	public Integer getMoveshard() {
		return moveshard;
	}

	public void setMoveshard(Integer moveshard) {
		this.moveshard = moveshard;
	}

	public Integer getMovesexpert() {
		return movesexpert;
	}

	public void setMovesexpert(Integer movesexpert) {
		this.movesexpert = movesexpert;
	}

	public Integer getTimeeasy() {
		return timeeasy;
	}

	public void setTimeeasy(Integer timeeasy) {
		this.timeeasy = timeeasy;
	}

	public Integer getTimemedium() {
		return timemedium;
	}

	public void setTimemedium(Integer timemedium) {
		this.timemedium = timemedium;
	}

	public Integer getTimehard() {
		return timehard;
	}

	public void setTimehard(Integer timehard) {
		this.timehard = timehard;
	}

	public Integer getTimeexpert() {
		return timeexpert;
	}

	public void setTimeexpert(Integer timeexpert) {
		this.timeexpert = timeexpert;
	}
}
