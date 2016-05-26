using System;

namespace db
{
	[Gtk.TreeNode (ListOnly=true)]
	public class UserNode : Gtk.TreeNode {

		public UserNode (int id, string username, string email)
		{
			UserID = id;
			Username = username;
			Email = email;
		}

		[Gtk.TreeNodeValue (Column=0)]
		public int UserID;

		[Gtk.TreeNodeValue (Column=1)]
		public string Username;

		[Gtk.TreeNodeValue (Column=2)]
		public string Email;
	}

	[Gtk.TreeNode (ListOnly=true)]
	public class PostNode : Gtk.TreeNode {

		public PostNode (int id, int authorID, string title, string text)
		{
			PostID = id;
			AuthorID = authorID;
			Title = title;
			Text = text;
		}

		[Gtk.TreeNodeValue (Column=0)]
		public int PostID;

		[Gtk.TreeNodeValue (Column=1)]
		public int AuthorID;

		[Gtk.TreeNodeValue (Column=2)]
		public string Title;

		[Gtk.TreeNodeValue (Column=3)]
		public string Text;
	}

	[Gtk.TreeNode (ListOnly=true)]
	public class TagNode : Gtk.TreeNode {

		public TagNode (int id, string title)
		{
			TagID = id;
			Title = title;
		}

		[Gtk.TreeNodeValue (Column=0)]
		public int TagID;

		[Gtk.TreeNodeValue (Column=1)]
		public string Title;
	}
}

