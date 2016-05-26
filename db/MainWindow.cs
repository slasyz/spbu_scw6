using System;
using System.Data;
using Gtk;
using MySql.Data.MySqlClient;
using db;

public partial class MainWindow: Gtk.Window
{
	private IDbConnection DBCon;

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

			Console.WriteLine ("[{0}] {1} {2}", id, username, email);

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

			nodeviewUsers.NodeStore.AddNode (new PostNode(id, authorID, title, text));
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

			nodeviewUsers.NodeStore.AddNode (new TagNode(id, title));
		}

		reader.Close ();
		reader = null;
	}

	protected void OnButtonUserAddClicked (object sender, EventArgs e)
	{
		
	}

	protected void OnButtonUserEditClicked (object sender, EventArgs e)
	{
		throw new NotImplementedException ();
	}

	protected void OnButtonPostAddClicked (object sender, EventArgs e)
	{
		throw new NotImplementedException ();
	}

	protected void OnButtonPostEditClicked (object sender, EventArgs e)
	{
		throw new NotImplementedException ();
	}

	protected void OnButtonTagAddClicked (object sender, EventArgs e)
	{
		throw new NotImplementedException ();
	}

	protected void OnButtonTagEditClicked (object sender, EventArgs e)
	{
		throw new NotImplementedException ();
	}
}
