# Eudora.Net
A brand new email client inspired by the once-dominant Qualcomm Eudora<sup>1</sup>
------

Eudora.Net is built from scratch with the goal of providing the look and feel of classic Eudora in an email client that is new and modern inside.

Fans of the original will find that the bulk of the UX is exactly where they expect to find it, with only minimal changes.  

### Notable features:
- WPF / .Net8 with liberal use of async programming. You should find Eudora.Net to be quite responsive.
- The entire datastore is encrypted with your unique key. Your key is stored in the Windows Credential Manager so you don't have to type it in.
- Using a cloud drive (Google, MS, etc) as the datastore root means that you can easily and securely access your data from any machine on which:
  1. You have Eudora.Net installed
  2. You log on to that PC with the same Microsoft account (your encryption key in the Credential Manager roams with you automatically)
- Uses the splendid [MailKit](https://mimekit.net/docs/html/Introduction.htm) for all core email client-server communications.
- Can import old mailboxes and address books from Eudora 7
- 

---

### To do:
- Localization
- 

---

### Notes:
1. Qualcomm is in no way responsible for this software. Please don't contact that company with feedback about Eudora.Net, lest they become angry and hurl rocks upon us.
