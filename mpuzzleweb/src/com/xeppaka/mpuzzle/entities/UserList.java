package com.xeppaka.mpuzzle.entities;

import java.util.ArrayList;
import java.util.List;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlTransient;

@XmlRootElement(name="users")
@XmlAccessorType(XmlAccessType.FIELD)
public class UserList {

	@XmlElement(name="user")
	private List<User> users;

	public UserList() {
		users = new ArrayList<User>();
		users.add(new User("1213", null));
		users.add(new User("1214", null));
		users.add(new User("1215", null));
		users.add(new User("1216", null));
	}
	public UserList(List<User> users) {
		this.users = users;
	}

	@XmlTransient
	public List<User> getUsers() {
		return users;
	}

	public void setUsers(List<User> users) {
		this.users = users;
	}
}
