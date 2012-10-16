package com.xeppaka.mpuzzle.entities;

import java.util.List;

import javax.jdo.annotations.IdGeneratorStrategy;
import javax.jdo.annotations.PersistenceCapable;
import javax.jdo.annotations.Persistent;
import javax.jdo.annotations.PrimaryKey;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlTransient;
import javax.xml.bind.annotation.XmlType;

import com.google.appengine.api.datastore.Key;
import com.google.appengine.api.datastore.KeyFactory;

@XmlType
@XmlAccessorType(XmlAccessType.PUBLIC_MEMBER)
@PersistenceCapable
public class User {

	@PrimaryKey
	@Persistent(valueStrategy = IdGeneratorStrategy.IDENTITY)
	private Key key;
	
	@Persistent
	private String username;
	
	@Persistent
	private List<GameResult> results;
	
	public User(String username, List<GameResult> results) {
		this.username = username;
		this.results = results;
	}

	@XmlElement
	public String getUsername() {
		return username;
	}

	public void setUsername(String username) {
		this.username = username;
	}

	@XmlElement
	public long getUserid() {
		return key.getId();
	}
	
	public void setUserid(long id) {
		key = KeyFactory.createKey(User.class.getName(), id);
	}

	@XmlTransient
	public List<GameResult> getResults() {
		return results;
	}

	public void setResults(List<GameResult> results) {
		this.results = results;
	}
}
