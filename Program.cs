using System;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

internal class Program
{
    private static SqlConnection sqlConnection;
    private static SqlCommand sqlCommand;
    private static string connectionString = "Server=localhost\\SQLEXPRESS;Database=DiaryV7;Trusted_Connection=True;";
    private static string query = "";
    private static void Main(string[] args)
    {

        sqlConnection = new SqlConnection(connectionString);
        sqlConnection.Open();
        Console.WriteLine("Change Your Emotion     - Hoş Geldiniz -     Change Your Emotion");
        while (true)
        {
            Console.WriteLine("1 - Kayıt Ol\n2 - Giriş Yap\n3 - Çıkış Yap");
            if (int.TryParse(Console.ReadLine(), out int choose))
            {
                switch (choose)
                {
                    case 1:
                        Console.Clear();
                        AddUser();
                        break;
                    case 2:
                        Console.Clear();
                        int userId = Login();
                        if (userId == -1)
                        {
                            Console.WriteLine("Kullanıcı Adı veya Şifre Hatalı.\n1 - Şifreni Yenile\n2 - Çıkış Yap\n3 - Ana Menü\n4 - Kayıt Ol");
                            choose = Convert.ToInt32(Console.ReadLine());
                            switch (choose)
                            {
                                case 1:
                                    Console.Clear();
                                    ResetPassword();
                                    break;
                                case 2:
                                    Console.Clear();
                                    return;
                                case 3:
                                    Console.Clear();
                                    break;
                                case 4:
                                    Console.Clear();
                                    AddUser();
                                    break;
                            }
                        }
                        else
                            MenuDetails(userId);
                        break;
                    case 3:
                        return;
                    default:
                        Console.Clear();
                        Console.WriteLine("**********       Hatalı Bir Tuşlama Yaptınız Lütfen Tekrar Deneyin       **********");
                        break;
                }
            }

        }
    }

    private static void MenuDetails(int userId)
    {
        while (true)
        {
            Menu();
            if (int.TryParse(Console.ReadLine(), out int choose))
            {
                switch (choose)
                {
                    case 1:
                        Console.Clear();
                        if (ControlTodaysDiary(userId)) AddDiary(userId);
                        else
                        {
                            Console.WriteLine("Bugünkü Hislerinizi Zaten Yazdınız. Yine de Hislerinizi Eklemek İster Misiniz?\n1 - Evet\n2 - Hayır");
                            choose = Convert.ToInt32(Console.ReadLine());
                            if (choose == 1) AddDiary(userId);
                        }

                        Console.WriteLine("Ana Menüye Dönmek İçin Herhangi Bir Tuşa Basın.");
                        Console.ReadLine();
                        Console.Clear();
                        break;
                    case 2:
                        Console.Clear();
                        ListDiarys(userId);
                        Console.WriteLine("Ana Menüye Dönmek İçin Herhangi Bir Tuşa Basın.");
                        Console.ReadLine();
                        Console.Clear();
                        break;
                    case 3:
                        Console.Clear();
                        Console.WriteLine("Tüm Hislerinizi Silmek İstediğinize Emin Misiniz ?\n1 - Evet\n2 - Hayır");
                        if (int.TryParse(Console.ReadLine(), out choose))
                        {
                            if (choose == 1) DeleteAllDiarys(userId);
                            else if (choose == 2) Console.Write("");
                            else Console.WriteLine("**********       Hatalı Bir Tuşlama Yaptınız Lütfen Tekrar Deneyin       **********");
                        }
                        Console.WriteLine("Ana Menüye Dönmek İçin Herhangi Bir Tuşa Basın.");
                        Console.ReadLine();
                        Console.Clear();
                        break;
                    case 4:
                        Console.Clear();
                        SearchDiary(userId);
                        break;
                    case 5:
                        Console.Clear();
                        Console.WriteLine("Çıkış Yapılıyor...");
                        return;
                    default:
                        Console.Clear();
                        Console.WriteLine("**********       Hatalı Bir Tuşlama Yaptınız Lütfen Tekrar Deneyin       **********");
                        break;
                }
            }
            else
            {
                Console.Clear();
                Console.WriteLine("**********       Hatalı Bir Tuşlama Yaptınız Lütfen Tekrar Deneyin       **********");
            }
        }
    }

    private static void Menu()
    {
        Console.WriteLine("1 - Bugünkü Hislerini Yaz");
        Console.WriteLine("2 - Yaşadığın Hisleri Listele");
        Console.WriteLine("3 - Tüm Hisleri Sil");
        Console.WriteLine("4 - Tarihe Göre Ara");
        Console.WriteLine("5 - Çıkış");
    }

    private static void ResetPassword()
    {
        bool isContinue = true;

        Console.Write("Kullanıcı Adı : ");
        string userNameInput = Console.ReadLine();
        query = "SELECT * FROM Users WHERE UserName = @UserName";
        using (sqlCommand = new SqlCommand(query, sqlConnection))
        {
            sqlCommand.Parameters.AddWithValue("@UserName", userNameInput);
            using (SqlDataReader reader = sqlCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    string userName = reader["UserName"].ToString();
                    string securityQuestion = reader["SecurityQuestion"].ToString();
                    string securityAnswer = reader["SecurityAnswer"].ToString();

                    Console.Write($"{securityQuestion} : ");
                    string securityAnswerInput = Console.ReadLine();
                    if (securityAnswer.Equals(securityAnswerInput))
                    {
                        reader.Close();
                        sqlCommand.Connection.Close();
                        string newPassword = "";
                        Console.Write("Yeni Şifre : ");
                        while (isContinue)
                        {
                            newPassword = Console.ReadLine();
                            if (newPassword.Length < 8) Console.WriteLine("Şifreniz 8 Karakterden Fazla Olmalıdır."); //Validation-2
                            if (!newPassword.Any(char.IsUpper)) Console.WriteLine("Şifreniz En Az 1 Büyük Harf İçermelidir."); //Validation-3
                            if (newPassword.Length >= 8 && newPassword.Any(char.IsUpper)) isContinue = false;
                        }

                        query = "UPDATE Users SET Password = @Password WHERE UserName=@UserName";
                        sqlConnection.Open();
                        using (sqlCommand = new SqlCommand(query, sqlConnection))
                        {
                            sqlCommand.Parameters.AddWithValue("@UserName", userName);
                            sqlCommand.Parameters.AddWithValue("@Password", EncodeBase64(newPassword));
                            ExecuteNonQuery("Şifre güncelleme", sqlCommand);
                            return;
                        }
                    }
                    else Console.WriteLine("Güvenlik Sorusunun Cevabı Yanlış!");
                }
            }
        }
    }

    private static int Login()
    {
        int userId = -1, userIdDb = 0;
        Console.Write("Kullanıcı Adınızı Giriniz : ");
        string userNameInput = Console.ReadLine();
        Console.Write("Şifrenizi Giriniz : ");
        string passwordInput = Console.ReadLine();

        query = "SELECT * FROM Users";
        using (sqlCommand = new SqlCommand(query, sqlConnection))
        using (SqlDataReader reader = sqlCommand.ExecuteReader())
        {
            while (reader.Read())
            {
                userIdDb = (int)reader["UserId"];
                string userName = reader["UserName"].ToString();
                string encodedPassword = reader["Password"].ToString();

                if ((userName == userNameInput) && DecodeBase64(encodedPassword) == passwordInput)
                {
                    return userIdDb; //Giriş Yapılacak
                }
            }
        }
        return userId; //Hiçbir şifre ile eşleşmedi 
    }

    private static bool ControlUserName(string userNameInput) //Validation-1
    {
        query = "SELECT UserName FROM Users";
        using (sqlCommand = new SqlCommand(query, sqlConnection))
        using (SqlDataReader reader = sqlCommand.ExecuteReader())
        {
            while (reader.Read())
            {
                string userName = reader["UserName"].ToString();

                if (userName.Equals(userNameInput)) return false; //Böyle bir kullanıcı kayıtlı
            }
        }
        return true;
    }

    private static void AddUser()
    {
        bool isContinue = true;
        Console.Write("Kullanıcı Adı : ");
        string userName = Console.ReadLine();
        if (ControlUserName(userName))
        {
            Console.Write("Ad : ");
            string name = Console.ReadLine();
            Console.Write("Soyad : ");
            string surname = Console.ReadLine();
            Console.Write("Güvenlik Sorusu : ");
            string securityQuestion = Console.ReadLine();
            Console.Write("Güvenlik Sorusunun Cevabı : ");
            string securityAnswer = Console.ReadLine();
            Console.Write("Şifre : ");
            string newPassword = "";

            while (isContinue)
            {
                newPassword = Console.ReadLine();
                if (newPassword.Length < 8) Console.WriteLine("Şifreniz 8 Karakterden Fazla Olmalıdır."); //Validation-2
                if (!newPassword.Any(char.IsUpper)) Console.WriteLine("Şifreniz En Az 1 Büyük Harf İçermelidir."); //Validation-3
                if (newPassword.Length >= 8 && newPassword.Any(char.IsUpper)) isContinue = false;
            }
            string password = EncodeBase64(newPassword);

            query = "INSERT INTO Users (UserName, Password, Name, Surname, SecurityQuestion, SecurityAnswer, DateOfRegister) VALUES (@UserName, @Password, @Name, @Surname, @SecurityQuestion, @SecurityAnswer, @DateOfRegister)";
            using (sqlCommand = new SqlCommand(query, sqlConnection))
            {
                sqlCommand.Parameters.AddWithValue("@UserName", userName);
                sqlCommand.Parameters.AddWithValue("@Name", name);
                sqlCommand.Parameters.AddWithValue("@Surname", surname);
                sqlCommand.Parameters.AddWithValue("@SecurityQuestion", securityQuestion);
                sqlCommand.Parameters.AddWithValue("@SecurityAnswer", securityAnswer);
                sqlCommand.Parameters.AddWithValue("@Password", password);
                sqlCommand.Parameters.AddWithValue("@DateOfRegister", DateTime.Now);
                ExecuteNonQuery("Öğrenci ekleme", sqlCommand);
            }
        }
        else
        {
            Console.Clear();
            while (true)
            {
                Console.WriteLine("Bu Kullanıcı Adı Alınmış.\n1 - Yeni Kullanıcı Adı\n2 - Şifremi Unuttum\n3 - Çıkış");
                int choose = Convert.ToInt32(Console.ReadLine());
                switch (choose)
                {
                    case 1:
                        AddUser();
                        break;
                    case 2:
                        Console.Clear();
                        ResetPassword();
                        break;
                    case 3:
                        return;
                    default:
                        Console.WriteLine("**********       Hatalı Bir Tuşlama Yaptınız Lütfen Tekrar Deneyin       **********");
                        break;
                }
            }
        }
    }

    private static bool ControlTodaysDiary(int userId)
    {
        DateTime dt = DateTime.Now;
        string today = dt.ToString("d");

        query = "SELECT Date FROM Diarys WHERE UserId = @UserId";

        using (sqlCommand = new SqlCommand(query, sqlConnection))
        {
            sqlCommand.Parameters.AddWithValue("@UserId", userId);
            using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
            {
                while (sqlDataReader.Read())
                {
                    string date = sqlDataReader["Date"].ToString();
                    if (date.Contains(today)) return false;
                }
            }
            return true;
        }
    }

    private static void AddDiary(int userId)
    {
        query = "INSERT INTO Diarys (Diary, Date, UserId, UpdateDate) VALUES (@Diary, @Date, @UserId, @UpdateDate)";
        Console.WriteLine("Bugün Neler Hissettiniz ? ");
        string diary = Console.ReadLine();

        using (sqlCommand = new SqlCommand(query, sqlConnection))
        {
            sqlCommand.Parameters.AddWithValue("@Diary", EncodeBase64(diary));
            sqlCommand.Parameters.AddWithValue("@Date", DateTime.Today);
            sqlCommand.Parameters.AddWithValue("@UserId", userId);
            sqlCommand.Parameters.AddWithValue("@UpdateDate", DateTime.Now);
            ExecuteNonQuery("Bugünkü hislerini ekleme", sqlCommand);
        }
    }

    private static void ListDiarys(int userId)
    {
        int countOfRows = 0;
        query = "SELECT * FROM Diarys WHERE UserId=@UserId";
        bool isContinue = true;
        using (sqlCommand = new SqlCommand(query, sqlConnection))
        {
            sqlCommand.Parameters.AddWithValue("@UserId", userId);
            using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
            {
                while (sqlDataReader.Read())
                {
                    int diaryId = (int)sqlDataReader["DiaryId"];
                    string encodedDiary = sqlDataReader["Diary"].ToString();
                    string date = sqlDataReader["Date"].ToString();
                    string updateDate = sqlDataReader["UpdateDate"].ToString();

                    string diary = DecodeBase64(encodedDiary);

                    if (isContinue)
                    {
                        Console.Clear();
                        Console.WriteLine("----------------------------");
                        Console.WriteLine(date);
                        Console.WriteLine(updateDate);
                        Console.WriteLine(diary);
                        Console.WriteLine("----------------------------");

                        Console.WriteLine("1 - Sonraki Hisler\n2 - Düzenle\n3 - Sil\n4 - Ana Menüye Dön");
                        int choose = Convert.ToInt32(Console.ReadLine());
                        switch (choose)
                        {
                            case 1:
                                isContinue = true;
                                break;
                            case 2:
                                Console.Clear();
                                sqlDataReader.Close();
                                UpdateDiary(diaryId);
                                return;
                            case 3:
                                sqlDataReader.Close();
                                DeleteDiary(diaryId);
                                return;
                            case 4:
                                Console.WriteLine("Ana Menüye Dönüyorsunuz.");
                                return;
                            default:
                                Console.WriteLine("Hatalı Tuşlama Yaptınız");
                                break;
                        }
                    }
                    countOfRows++;
                }
                if (countOfRows == 0) Console.WriteLine("Duygu Bulunamadı");
                else if (sqlDataReader.HasRows) Console.WriteLine("Son Duygudasınız.");
            }
            if (countOfRows == 0) Console.WriteLine("Duygu bulunamadı.");
        }
    }

    private static void UpdateDiary(int diaryId)
    {
        Console.Write("Yeni Hisler : ");
        string diary = Console.ReadLine();
        query = "UPDATE Diarys SET Diary = @Diary WHERE DiaryId = @DiaryId";
        using (sqlCommand = new SqlCommand(query, sqlConnection))
        {
            sqlCommand.Parameters.AddWithValue("@DiaryId", diaryId);
            sqlCommand.Parameters.AddWithValue("@Diary", EncodeBase64(diary));
            sqlCommand.Parameters.AddWithValue("@UpdateDate", DateTime.Now);
            ExecuteNonQuery("His güncelleme", sqlCommand);
        }
    }

    private static void DeleteDiary(int diaryId)
    {
        query = "DELETE FROM Diarys WHERE DiaryId = @DiaryId";
        using (sqlCommand = new SqlCommand(query, sqlConnection))
        {
            sqlCommand.Parameters.AddWithValue("@DiaryId", diaryId);
            ExecuteNonQuery("Hisleri silme", sqlCommand);
        }
    }

    private static void DeleteAllDiarys(int userId)
    {
        query = "DELETE FROM Diarys WHERE UserId = @UserID";
        using (sqlCommand = new SqlCommand(query, sqlConnection))
        {
            sqlCommand.Parameters.AddWithValue("@UserId", userId);
            ExecuteNonQuery("Hissettiklerini silme ", sqlCommand);
        }
    }

    private static void ExecuteNonQuery(string operation, SqlCommand sqlCommand)
    {
        sqlCommand.ExecuteNonQuery();
        Console.WriteLine($"{operation} işlemi başarıyla tamamlandı!");
    }

    private static void SearchDiary(int userId)
    {
        bool isContinue = true;
        Console.WriteLine("Hangi Tarihin Hislerini Açmak İstiyorsunuz ?");
        query = "SELECT * FROM Diarys WHERE Date LIKE @Search AND UserId=@UserId";
        string searchDate = Console.ReadLine();
        Console.Clear();
        using (sqlCommand = new SqlCommand(query, sqlConnection))
        {
            sqlCommand.Parameters.AddWithValue("@UserId", userId);
            sqlCommand.Parameters.AddWithValue("@Search", "%" + searchDate + "%");
            using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
            {
                while (sqlDataReader.Read())
                {
                    int diaryId = (int)sqlDataReader["DiaryId"];
                    string encodedDiary = sqlDataReader["Diary"].ToString();
                    string date = sqlDataReader["Date"].ToString();
                    string updateDate = sqlDataReader["UpdateDate"].ToString();

                    string diary = DecodeBase64(encodedDiary);

                    if (isContinue)
                    {
                        Console.Clear();
                        Console.WriteLine("----------------------------");
                        Console.WriteLine(date);
                        Console.WriteLine(updateDate);
                        Console.WriteLine(diary);
                        Console.WriteLine("----------------------------");

                        Console.WriteLine("1 - Sonraki Hisler\n2 - Düzenle\n3 - Sil\n4 - Ana Menüye Dön");
                        int choose = Convert.ToInt32(Console.ReadLine());
                        switch (choose)
                        {
                            case 1:
                                isContinue = true;
                                break;
                            case 2:
                                Console.Clear();
                                sqlDataReader.Close();
                                UpdateDiary(diaryId);
                                return;
                            case 3:
                                sqlDataReader.Close();
                                DeleteDiary(diaryId);
                                return;
                            case 4:
                                Console.WriteLine("Ana Menüye Dönüyorsunuz.");
                                return;
                            default:
                                Console.WriteLine("Hatalı Tuşlama Yaptınız");
                                break;
                        }
                    }
                }
            }
        }
    }

    private static string EncodeBase64(string input)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        return Convert.ToBase64String(inputBytes);
    }

    private static string DecodeBase64(string input)
    {
        byte[] base64Bytes = Convert.FromBase64String(input);
        return Encoding.UTF8.GetString(base64Bytes);
    }
}