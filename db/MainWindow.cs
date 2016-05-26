using System;
using System.Data;
using Gtk;
using MySql.Data.MySqlClient;
using db;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;

public partial class MainWindow: Gtk.Window
{
	private MySqlConnection DBCon;

	public MainWindow () : base (Gtk.WindowType.Toplevel)
	{
		Build ();

		nodeviewUsers.NodeStore = new Gtk.NodeStore (typeof (UserNode));
		nodeviewUsers.AppendColumn("ID", new Gtk.CellRendererText (), "text", 0);
		nodeviewUsers.AppendColumn("Username", new Gtk.CellRendererText (), "text", 1);
		nodeviewUsers.AppendColumn("E-mail", new Gtk.CellRendererText (), "text", 2);

		nodeviewPosts.NodeStore = new Gtk.NodeStore (typeof (PostNode));
		nodeviewPosts.AppendColumn("ID", new Gtk.CellRendererText (), "text", 0);
		nodeviewPosts.AppendColumn("Author ID", new Gtk.CellRendererText (), "text", 1);
		nodeviewPosts.AppendColumn("Title", new Gtk.CellRendererText (), "text", 2);
		nodeviewPosts.AppendColumn("Text", new Gtk.CellRendererText (), "text", 3);

		nodeviewTags.NodeStore = new Gtk.NodeStore (typeof (TagNode));
		nodeviewTags.AppendColumn("ID", new Gtk.CellRendererText (), "text", 0);
		nodeviewTags.AppendColumn("Title", new Gtk.CellRendererText (), "text", 1);

		DatabaseConnect ();
		DatabaseUpdate ();
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		DBCon.Close ();
		a.RetVal = true;
	}

	protected void OnButtonUpdateClicked (object sender, EventArgs e)
	{
		DatabaseUpdate ();
	}

	protected void DatabaseConnect() {
		string connectionString =
			"Server=localhost;" +
			"Database=scw;" +
			"User ID=scw;" +
			"Password=scw;" +
			"Pooling=false";
		DBCon = new MySqlConnection(connectionString);
		DBCon.Open();
	}

	protected void DatabaseUpdate() {
		IDbCommand dbcmd;
		IDataReader reader;

		// Read users
		nodeviewUsers.NodeStore.Clear();
		dbcmd = DBCon.CreateCommand();
		dbcmd.CommandText = "SELECT * " +
			"FROM users";
		
		reader = dbcmd.ExecuteReader();
		while(reader.Read()) {
			int id = (int) reader["id"];
			string username = (string) reader["username"];
			string email = (string) reader["email"];

			nodeviewUsers.NodeStore.AddNode (new UserNode (id, username, email));
		}

		reader.Close ();
		reader = null;

		// Read posts
		nodeviewPosts.NodeStore.Clear();
		dbcmd = DBCon.CreateCommand();
		dbcmd.CommandText = "SELECT * " +
			"FROM posts";

		reader = dbcmd.ExecuteReader();
		while(reader.Read()) {
			int id = (int) reader["id"];
			int authorID = (int) reader["author_id"];
			string title = (string) reader["title"];
			string text = (string) reader["text"];

			nodeviewPosts.NodeStore.AddNode (new PostNode(id, authorID, title, text));
		}

		reader.Close ();
		reader = null;

		// Read tags
		nodeviewTags.NodeStore.Clear();
		dbcmd = DBCon.CreateCommand();
		dbcmd.CommandText = "SELECT * " +
			"FROM tags";

		reader = dbcmd.ExecuteReader();
		while(reader.Read()) {
			int id = (int) reader["id"];
			string title = (string) reader["title"];

			nodeviewTags.NodeStore.AddNode (new TagNode(id, title));
		}

		reader.Close ();
		reader = null;
	}

	public static string CalculateMD5Hash(string input)
	{
		// step 1, calculate MD5 hash from input
		MD5 md5 = System.Security.Cryptography.MD5.Create();
		byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
		byte[] hash = md5.ComputeHash(inputBytes);

		// step 2, convert byte array to hex string
		StringBuilder sb = new StringBuilder();

		for (int i = 0; i < hash.Length; i++)
		{
			sb.Append(hash[i].ToString("X2"));
		}

		return sb.ToString();
	}

	public bool IsAuthorExist(int id)
	{
		MySqlCommand dbcmd = DBCon.CreateCommand();
		dbcmd.CommandText = "SELECT COUNT(*) FROM users WHERE (id = @id)";  // Set the insert statement
		dbcmd.Parameters.AddWithValue("@id", id);

		int cnt = int.Parse(Convert.ToString(dbcmd.ExecuteScalar()));
		return cnt > 0;
	}

	public bool IsAuthorExist(string id)
	{
		return IsAuthorExist (int.Parse (id));
	}

	public bool IsPostTagExist(long postID, long tagID)
	{
		MySqlCommand dbcmd = DBCon.CreateCommand();
		dbcmd.CommandText = "SELECT COUNT(*) FROM posts_tags WHERE (post_id = @post_id AND tag_id = @tag_id)";
		dbcmd.Parameters.AddWithValue("@post_id", postID);
		dbcmd.Parameters.AddWithValue("@tag_id", tagID);

		int cnt = int.Parse(Convert.ToString(dbcmd.ExecuteScalar()));
		return cnt > 0;
	}

	public bool IsPostTagExist(long postID, string tagTitle, out long tagID)
	{
		IDataReader reader;

		MySqlCommand dbcmd = DBCon.CreateCommand();
		dbcmd.CommandText = "SELECT id FROM tags WHERE (title = @title)";
		dbcmd.Parameters.AddWithValue("@title", tagTitle);

		reader = dbcmd.ExecuteReader();
		while (reader.Read()) {
			tagID = (int)reader ["id"];
			reader.Close ();

			return IsPostTagExist (postID, tagID);
		}
		reader.Close ();

		// Create a new tag (not a post-tag connection)
		dbcmd = new MySqlCommand();
		dbcmd.Connection = DBCon;

		dbcmd.CommandText = "INSERT INTO tags(title) VALUES(@title)";
		dbcmd.Prepare ();
		dbcmd.Parameters.AddWithValue ("@title", tagTitle);

		dbcmd.ExecuteNonQuery ();
		tagID = dbcmd.LastInsertedId;

		return false;
	}

	public string GetPostTags(int id)
	{
		IDataReader reader;
		List<string> tags = new List<string>();

		MySqlCommand dbcmd = DBCon.CreateCommand();
		dbcmd.CommandText = "SELECT tags.title " +
			                "FROM posts_tags " +
							"JOIN tags " +
							"    ON tags.id = posts_tags.tag_id " +
			              	"WHERE (post_id = @post_id)";
		dbcmd.Parameters.AddWithValue("@post_id", id);

		reader = dbcmd.ExecuteReader();
		while (reader.Read()) {
			tags.Add ((string)reader ["title"]);
		}
		reader.Close ();

		return string.Join (",", tags);
	}

	public void SetPostTags(long id, string tags)
	{
		List<string> tagsList;
		if (tags == "")
			tagsList = new List<string> ();
		else
			tagsList = new List<string> (tags.Split(','));
		HashSet<string> tagsHash = new HashSet<string> (tagsList);
		long tagID;

		foreach (string tag in tagsHash) {
			if (!IsPostTagExist (id, tag, out tagID)) {
				MySqlCommand dbcmd = new MySqlCommand();
				dbcmd.Connection = DBCon;

				dbcmd.CommandText = "INSERT INTO posts_tags(post_id, tag_id) VALUES(@post_id, @tag_id)";
				dbcmd.Prepare ();

				dbcmd.Parameters.AddWithValue ("@post_id", id);
				dbcmd.Parameters.AddWithValue ("@tag_id", tagID);

				dbcmd.ExecuteNonQuery ();
			}
		}
	}

	public void SetPostTags(string id, string tags)
	{
		SetPostTags (int.Parse (id), tags);
		return;
	}

	public int GetAuthorPostsCount(int id)
	{
		MySqlCommand dbcmd = new MySqlCommand();
		dbcmd.Connection = DBCon;

		dbcmd.CommandText = "SELECT COUNT(*) FROM posts WHERE (author_id = @author_id)";
		dbcmd.Prepare ();
		dbcmd.Parameters.AddWithValue ("@author_id", id);

		return int.Parse(Convert.ToString(dbcmd.ExecuteScalar()));
	}

	public int GetAuthorPostsCount(string id)
	{
		return int.Parse(id);
	}

	protected void OnButtonUserAddClicked (object sender, EventArgs e)
	{
		if (vboxUserEdit.Visible && entryUserEditID.Text == "")
			vboxUserEdit.Hide ();
		else {
			entryUserEditID.Text = "";
			entryUserEditEmail.Text = "";
			entryUserEditUsername.Text = "";
			entryUserEditPassword.Text = "";
			vboxUserEdit.Show ();
		}
	}

	protected void OnButtonUserEditClicked (object sender, EventArgs e)
	{
		UserNode selected = (UserNode) nodeviewUsers.NodeSelection.SelectedNode;
		if (selected == null)
			return;
		if (vboxUserEdit.Visible && selected.UserID.ToString () == entryUserEditID.Text) {
			vboxUserEdit.Hide ();
			return;
		}
		
		vboxUserEdit.Show ();
		entryUserEditID.Text = selected.UserID.ToString();
		entryUserEditEmail.Text = selected.Email;
		entryUserEditPassword.Text = "";
		entryUserEditUsername.Text = selected.Username;
	}

	protected void OnButtonPostAddClicked (object sender, EventArgs e)
	{
		if (vboxPostEdit.Visible && entryPostEditID.Text == "")
			vboxPostEdit.Hide ();
		else {
			entryPostEditID.Text = "";
			entryPostEditAuthor.Text = "";
			entryPostEditTags.Text = "";
			entryPostEditTitle.Text = "";
			textviewPostEditText.Buffer.Text = "";
			vboxPostEdit.Show ();
		}
	}

	protected void OnButtonPostEditClicked (object sender, EventArgs e)
	{
		PostNode selected = (PostNode) nodeviewPosts.NodeSelection.SelectedNode;
		if (selected == null)
			return;
		if (vboxPostEdit.Visible && selected.PostID.ToString () == entryPostEditID.Text) {
			vboxPostEdit.Hide ();
			return;
		}

		vboxPostEdit.Show ();
		entryPostEditID.Text = selected.PostID.ToString ();
		entryPostEditAuthor.Text = selected.AuthorID.ToString ();
		entryPostEditTags.Text = GetPostTags(selected.PostID);
		entryPostEditTitle.Text = selected.Title;
		textviewPostEditText.Buffer.Text = selected.Text;
	}

	protected void OnButtonTagAddClicked (object sender, EventArgs e)
	{
		if (vboxTagEdit.Visible && entryTagEditID.Text == "")
			vboxTagEdit.Hide ();
		else {
			entryTagEditID.Text = "";
			entryTagEditTitle.Text = "";
			vboxTagEdit.Show ();
		}
	}

	protected void OnButtonTagEditClicked (object sender, EventArgs e)
	{
		TagNode selected = (TagNode) nodeviewTags.NodeSelection.SelectedNode;
		if (selected == null)
			return;
		if (vboxTagEdit.Visible && selected.TagID.ToString () == entryTagEditID.Text) {
			vboxTagEdit.Hide ();
			return;
		}

		vboxTagEdit.Show ();
		entryTagEditID.Text = selected.TagID.ToString ();
		entryTagEditTitle.Text = selected.Title;
	}

	protected void OnButtonTagEditCancelClicked (object sender, EventArgs e)
	{
		vboxTagEdit.Hide ();
	}

	protected void OnButtonPostEditCancelClicked (object sender, EventArgs e)
	{
		vboxPostEdit.Hide ();
	}

	protected void OnButtonUserEditCancelClicked (object sender, EventArgs e)
	{
		vboxUserEdit.Hide ();
	}

	protected void OnButtonUserEditOKClicked (object sender, EventArgs e)
	{
		MySqlCommand dbcmd = new MySqlCommand();
		dbcmd.Connection = DBCon;

		if (entryUserEditID.Text == "") {
			try {
				dbcmd.CommandText = "INSERT INTO users(username, email, password) VALUES(@username, @email, @password)";
				dbcmd.Prepare ();

				dbcmd.Parameters.AddWithValue ("@username", entryUserEditUsername.Text);
				dbcmd.Parameters.AddWithValue ("@email", entryUserEditEmail.Text);
				dbcmd.Parameters.AddWithValue ("@password", CalculateMD5Hash(entryUserEditPassword.Text));

				dbcmd.ExecuteNonQuery ();

				vboxUserEdit.Hide();
				DatabaseUpdate();
			} catch (MySqlException ex) {
				Console.WriteLine("Error: {0}",  ex.ToString());
			}
		} else {
			try {
				if (entryUserEditPassword.Text != "")
					dbcmd.CommandText = "UPDATE users SET username = @username, email = @email, password = @password WHERE (id = @id)";
				else
					dbcmd.CommandText = "UPDATE users SET username = @username, email = @email WHERE (id = @id)";
				dbcmd.Prepare ();

				dbcmd.Parameters.AddWithValue ("@id", entryUserEditID.Text);
				dbcmd.Parameters.AddWithValue ("@username", entryUserEditUsername.Text);
				dbcmd.Parameters.AddWithValue ("@email", entryUserEditEmail.Text);
				dbcmd.Parameters.AddWithValue ("@password", CalculateMD5Hash(entryUserEditPassword.Text));

				dbcmd.ExecuteNonQuery ();

				vboxUserEdit.Hide();
				DatabaseUpdate();
			} catch (MySqlException ex) {
				Console.WriteLine("Error: {0}",  ex.ToString());
			}
		}
	}

	protected void OnButtonPostEditOKClicked (object sender, EventArgs e)
	{
		MySqlCommand dbcmd = new MySqlCommand();
		dbcmd.Connection = DBCon;

		if (entryPostEditID.Text == "") {
			try {
				dbcmd.CommandText = "INSERT INTO posts(author_id, title, text) VALUES(@author_id, @title, @text)";
				dbcmd.Prepare ();

				if (!IsAuthorExist(entryPostEditAuthor.Text))
					return; // TODO: error report
				dbcmd.Parameters.AddWithValue ("@author_id", entryPostEditAuthor.Text);
				dbcmd.Parameters.AddWithValue ("@title", entryPostEditTitle.Text);
				dbcmd.Parameters.AddWithValue ("@text", textviewPostEditText.Buffer.Text);

				dbcmd.ExecuteNonQuery ();

				SetPostTags(dbcmd.LastInsertedId, entryPostEditTags.Text);

				vboxPostEdit.Hide();
				DatabaseUpdate();
			} catch (MySqlException ex) {
				Console.WriteLine("Error: {0}",  ex.ToString());
			}
		} else {
			try {
				dbcmd.CommandText = "UPDATE posts SET author_id = @author_id, title = @title, text = @text WHERE (id = @id)";
				dbcmd.Prepare ();

				if (!IsAuthorExist(entryPostEditAuthor.Text))
					return; // TODO: error report
				dbcmd.Parameters.AddWithValue ("@author_id", entryPostEditAuthor.Text);
				dbcmd.Parameters.AddWithValue ("@title", entryPostEditTitle.Text);
				dbcmd.Parameters.AddWithValue ("@text", textviewPostEditText.Buffer.Text);
				dbcmd.Parameters.AddWithValue ("@id", int.Parse(entryPostEditID.Text));

				dbcmd.ExecuteNonQuery ();

				SetPostTags(entryPostEditID.Text, entryPostEditTags.Text);

				vboxPostEdit.Hide();
				DatabaseUpdate();
			} catch (MySqlException ex) {
				Console.WriteLine("Error: {0}",  ex.ToString());
			}
		}
	}

	protected void OnButtonTagEditOKClicked (object sender, EventArgs e)
	{
		MySqlCommand dbcmd = new MySqlCommand();
		dbcmd.Connection = DBCon;

		if (entryTagEditID.Text == "") {
			try {
				dbcmd.CommandText = "INSERT INTO tags(title) VALUES(@title)";
				dbcmd.Prepare ();

				dbcmd.Parameters.AddWithValue ("@title", entryTagEditTitle.Text);

				dbcmd.ExecuteNonQuery ();

				vboxTagEdit.Hide();
				DatabaseUpdate();
			} catch (MySqlException ex) {
				Console.WriteLine("Error: {0}",  ex.ToString());
			}
		} else {
			try {
				dbcmd.CommandText = "UPDATE tags SET title = @title WHERE (id = @id)";
				dbcmd.Prepare ();

				dbcmd.Parameters.AddWithValue ("@id", entryTagEditID.Text);
				dbcmd.Parameters.AddWithValue ("@title", entryTagEditTitle.Text);

				dbcmd.ExecuteNonQuery ();

				vboxTagEdit.Hide();
				DatabaseUpdate();
			} catch (MySqlException ex) {
				Console.WriteLine("Error: {0}",  ex.ToString());
			}
		}
	}

	protected void OnButtonUserDeleteClicked (object sender, EventArgs e)
	{
		UserNode selected = (UserNode) nodeviewUsers.NodeSelection.SelectedNode;
		if (selected == null)
			return;
		
		MySqlCommand dbcmd = new MySqlCommand();
		dbcmd.Connection = DBCon;

		if (GetAuthorPostsCount (selected.UserID) > 0)
			return; // TODO: error reporting

		try {
			dbcmd.CommandText = "DELETE FROM users WHERE (id = @id)";
			dbcmd.Prepare ();
			dbcmd.Parameters.AddWithValue ("@id", selected.UserID);
			dbcmd.ExecuteNonQuery ();

			DatabaseUpdate();
		} catch (MySqlException ex) {
			Console.WriteLine("Error: {0}",  ex.ToString());
		}
	}

	protected void OnButtonPostDeleteClicked (object sender, EventArgs e)
	{
		PostNode selected = (PostNode) nodeviewPosts.NodeSelection.SelectedNode;
		if (selected == null)
			return;

		// Delete post-tag connections to this post
		MySqlCommand dbcmd = new MySqlCommand();
		dbcmd.Connection = DBCon;
		try {
			dbcmd.CommandText = "DELETE FROM posts_tags WHERE (post_id = @id)";
			dbcmd.Prepare ();
			dbcmd.Parameters.AddWithValue ("@id", selected.PostID);
			dbcmd.ExecuteNonQuery ();

			DatabaseUpdate();
		} catch (MySqlException ex) {
			Console.WriteLine("Error: {0}",  ex.ToString());
		}

		// Delete this post

		dbcmd = new MySqlCommand();
		dbcmd.Connection = DBCon;
		try {
			dbcmd.CommandText = "DELETE FROM posts WHERE (id = @id)";
			dbcmd.Prepare ();
			dbcmd.Parameters.AddWithValue ("@id", selected.PostID);
			dbcmd.ExecuteNonQuery ();

			DatabaseUpdate();
		} catch (MySqlException ex) {
			Console.WriteLine("Error: {0}",  ex.ToString());
		}
	}

	protected void OnButtonTagDeleteClicked (object sender, EventArgs e)
	{
		TagNode selected = (TagNode) nodeviewTags.NodeSelection.SelectedNode;
		if (selected == null)
			return;

		// Delete post-tag connections to this tag
		MySqlCommand dbcmd = new MySqlCommand();
		dbcmd.Connection = DBCon;
		try {
			dbcmd.CommandText = "DELETE FROM posts_tags WHERE (tag_id = @id)";
			dbcmd.Prepare ();
			dbcmd.Parameters.AddWithValue ("@id", selected.TagID);
			dbcmd.ExecuteNonQuery ();

			DatabaseUpdate();
		} catch (MySqlException ex) {
			Console.WriteLine("Error: {0}",  ex.ToString());
		}
			
		dbcmd = new MySqlCommand();
		dbcmd.Connection = DBCon;
		try {
			dbcmd.CommandText = "DELETE FROM tags WHERE (id = @id)";
			dbcmd.Prepare ();
			dbcmd.Parameters.AddWithValue ("@id", selected.TagID);
			dbcmd.ExecuteNonQuery ();

			DatabaseUpdate();
		} catch (MySqlException ex) {
			Console.WriteLine("Error: {0}",  ex.ToString());
		}
	}
}
