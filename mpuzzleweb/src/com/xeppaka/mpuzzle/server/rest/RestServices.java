package com.xeppaka.mpuzzle.server.rest;

import java.util.ArrayList;
import java.util.List;

import javax.jdo.PersistenceManager;
import javax.jdo.Query;
import javax.ws.rs.GET;
import javax.ws.rs.Path;
import javax.ws.rs.Produces;

import com.xeppaka.mpuzzle.datastore.PMF;
import com.xeppaka.mpuzzle.entities.User;
import com.xeppaka.mpuzzle.entities.UserList;

@Path("")
public class RestServices {

	@Path("/user")
	@GET
	@Produces("application/xml")
	public UserList getUsers() {
		PersistenceManager pm = PMF.get().getPersistenceManager();
		List<User> users = null;
		
		try {
			Query q = pm.newQuery(User.class);
			users = (List<User>)q.execute();
		} finally {
			pm.close();
		}
		
		if (users != null)
			return new UserList(users);
		else
			return new UserList();
	}
	
	@Path("/createuser")
	@GET
	@Produces("text/plain")
	public String createUser() {
		PersistenceManager pm = PMF.get().getPersistenceManager();
		
		try {
			User user = new User("user555", null);
			pm.makePersistent(user);
		} finally {
			pm.close();
		}
		
		return "finished";
	}	
}
