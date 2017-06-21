using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Questions
{
    public class Profile
    {
        private readonly IConsole console;

        public Profile(IConsole console)
        {
            this.console = console;
        }

        private static string pathFiles= "Анкеты";


        readonly List<Question> questions = new List<Question>()
        {
            new Question
            {
                Title = "ФИО",
                Format = s => { },
                Header = "ФИО",
                FileTitle = true
            },
            new Question
            {
                Title = "Дата рождения (Формат ДД.ММ.ГГГГ)",
                Format = s =>
                {
                    DateTime birthDateExact;
                    if (!DateTime.TryParse(s, out birthDateExact))
                    {
                        throw new InvalidOperationException("Неверный формат даты");
                    }
                },
                Header = "Дата рождения"
            },

            new Question
            {
                Title = "Любимый язык программирования PHP, JavaScript, C, C++, Java, C#, Python, Ruby",
                Format = s =>
                {
                    if (!new[]
                            {"PHP", "JavaScript", "C", "C++", "Java", "C#", "Python", "Ruby"}
                        .Contains(s))
                        throw new InvalidOperationException("Некорректный любимый язык программирования");
                },
                Header = "Любимый язык программирования"
            },
            new Question
            {
                Title = "Опыт программирования на указанном языке (Полных лет)",
                Format = s =>
                {
                    int expInt;
                    if (!int.TryParse(s, out expInt))
                        throw new InvalidOperationException("Неверное число - лет опыта");
                },
                Header = "Опыт программирования на указанном языке"
            },
            new Question
            {
                Title = "Мобильный телефон",
                Format = s => { },
                Header = "Мобильный телефон"
            }
        };


        public void StartWork()
        {
            try
            {
                console.WriteLine("Выберите действие:");

                var input = console.ReadLine();

                if (input == "-new_profile")
                {
                    CreateNewProfile();
                }
                else if (input == "-statistics")
                {
                    BuildStatistics();
                }
                else if (input == "-exit")
                {
                    console.WriteLine("Выход из программы");
                    return;
                }
                else if (input.StartsWith("-delete"))
                {
                    Delete(input);
                }
                else if (input.StartsWith("-find"))
                {
                    Find(input);
                }
                else if (input == "-list")
                {
                    List();
                }
                else if (input.StartsWith("-zip"))
                {
                    Zip(input);
                }
                else if (input.StartsWith("-list_today"))
                {
                    ListToday();
                }
                else if (input == "-help")
                {
                    console.WriteLine(commands);
                }
                else
                {
                    console.WriteLine("Команда не найдена");
                }
            }
            catch (InvalidOperationException ex)
            {
                console.WriteLine(ex.Message);
            }
            StartWork();
        }

        private void ListToday()
        {
            var files = Directory.GetFiles(pathFiles);
            if (!Directory.Exists(pathFiles) || files.Length == 0)
                throw new InvalidOperationException("Анкеты не найдены");
            foreach (var file in files)
            {
                var date = File.ReadAllLines(file).First(x => x.StartsWith("Анкета заполнена: "));
                var dateTime = DateTime.ParseExact(date, "Анкета заполнена: dd.MM.yyyy",
                    DateTimeFormatInfo.CurrentInfo);
                if (dateTime.Date == DateTime.Today.Date)
                    console.WriteLine(file);
            }
        }

        private void Zip(string input)
        {
            var s = input.Replace("Анкеты\\", "");
            s = input.Replace("-zip", "").Trim();
            var outputPath = input.Split(' ').Last();
            var fileName = Path.Combine(pathFiles, s.Replace(outputPath, "")).Trim();
            var files = Directory.GetFiles(pathFiles);
            if (!Directory.Exists(pathFiles) || !File.Exists(fileName))
                throw new InvalidOperationException("Анкета не найдена");
            foreach (var file in files)
                console.WriteLine(file);

            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);
            var archiveFileName = Path.Combine(Path.GetFileName(outputPath),
                Path.GetFileNameWithoutExtension(fileName) + ".zip");
            using (var zip = ZipFile.Open(archiveFileName, ZipArchiveMode.Create))
                zip.CreateEntryFromFile(fileName, Path.GetFileName(fileName));
        }

        private void List()
        {
            var files = Directory.GetFiles(pathFiles);
            if (!Directory.Exists(pathFiles) || files.Length == 0)
                throw new InvalidOperationException("Анкеты не найдены");
            foreach (var file in files)
                console.WriteLine(file);
        }

        private void Find(string input)
        {
            var file = Path.Combine(pathFiles, input.Replace("-find ", ""));
            if (!Directory.Exists(Path.GetDirectoryName(file)) || !File.Exists(file))
            {
                throw new InvalidOperationException("Анкета не найдена");
            }
            console.WriteLine(File.ReadAllText(file));
        }

        private static void Delete( string input)
        {
            var file = Path.Combine(pathFiles, input.Replace("-delete ", ""));
            if (!Directory.Exists(Path.GetDirectoryName(file)) || !File.Exists(file))
            {
                throw new InvalidOperationException("Анкета не найдена");
            }
            File.Delete(file);
        }

        private void BuildStatistics()
        {
            var files = Directory
                .GetFiles(pathFiles)
                .ToDictionary(Path.GetFileNameWithoutExtension, File.ReadAllLines);
            var oldList = new List<int>();
            var langList = new List<string>();
            var expList = new List<Tuple<string, int>>();
            foreach (var file in files)
            {
                var birth = file.Value.First(x => x.StartsWith("2. Дата рождения: "));
                birth = birth.Replace("2. Дата рождения: ", "");
                DateTime date;
                if (DateTime.TryParse(birth, out date))
                {
                    var age = DateTime.Today.Year - date.Year;
                    oldList.Add(age);
                }
                else
                    throw new InvalidOperationException("неверная дата рождения");
                var lang = file.Value.First(x => x.StartsWith("3. Любимый язык программирования: "));
                langList.Add(lang.Replace("3. Любимый язык программирования: ", ""));

                var exp = file.Value.First(x => x.StartsWith("4. Опыт программирования на указанном языке: "));
                exp = exp.Replace("4. Опыт программирования на указанном языке: ", "");
                expList.Add(new Tuple<string, int>(file.Key, int.Parse(exp)));
            }
            Array.Sort(oldList.ToArray());
            console.WriteLine("Средний возраст всех опрошенных: " + oldList[oldList.Count / 2]);

            var langM = langList.GroupBy(x => x).OrderByDescending(x => x.Count()).First().Key;
            console.WriteLine("Самый популярный язык программирования: " + langM);

            var experienced = expList.OrderByDescending(x => x.Item2).First().Item1.Replace(".txt", "");
            console.WriteLine("Самый опытный программист: " + experienced);
        }

        public void CreateNewProfile()
        {
            string fileTitle = null;
            var sb = new List<string>();
            for (var i = 0; i < questions.Count; i++)
            {
                var question = questions[i];
                console.WriteLine(question.Title);
                var value = console.ReadLine();
                if (value.StartsWith("-goto_question"))
                {
                    var number = value.Replace("-goto_question ", "");
                    i = int.Parse(number) - 2;
                    sb.RemoveRange(i + 1, sb.Count - (i + 1));
                    continue;
                }
                if (value.StartsWith("-goto_prev_question"))
                {
                    i = i - 2;
                    sb.RemoveAt(i + 1);
                    continue;
                }
                if (value.StartsWith("-restart_profile"))
                {
                    i = -1;
                    sb.RemoveAll(x => true);
                    continue;
                }
                question.Format(value);
                sb.Add($"{i + 1}. {question.Header}: {value}");
                if (question.FileTitle)
                    fileTitle = value + ".txt";
            }
            console.WriteLine("Выберите действие:");

            var action = console.ReadLine();

            if (action == "-save")
            {
                Directory.CreateDirectory(pathFiles);
                sb.Add("");
                sb.Add($"Анкета заполнена: {DateTime.Now:dd.MM.yyyy}");

                StringBuilder s = new StringBuilder();
                foreach (var ss in sb)
                {
                    s.AppendLine(ss);
                }
                File.WriteAllText($"Анкеты\\{fileTitle}", s.ToString());
            }
        }


        private string commands = @"cmd: -new_profile - Заполнить новую анкету
cmd: -statistics - Показать статистику всех заполненных анкет
cmd: -save - Сохранить заполненную анкету
cmd: -goto_question <Номер вопроса> - Вернуться к указанному вопросу (Команда доступна только при заполнении анкеты, вводится вместо ответа на любой вопрос)
cmd: -goto_prev_question - Вернуться к предыдущему вопросу (Команда доступна только при заполнении анкеты, вводится вместо ответа на любой вопрос)
cmd: -restart_profile - Заполнить анкету заново (Команда доступна только при заполнении анкеты, вводится вместо ответа на любой вопрос)
cmd: -find <Имя файла анкеты> - Найти анкету и показать данные анкеты в консоль
cmd: -delete <Имя файла анкеты> - Удалить указанную анкету
cmd: -list - Показать список названий файлов всех сохранённых анкет
cmd: -list_today - Показать список названий файлов всех сохранённых анкет, созданных сегодня
cmd: -zip <Имя файла анкеты> <Путь для сохранения архива> - Запаковать указанную анкету в архив и сохранить архив по указанному пути
cmd: -help - Показать список доступных команд с описанием
cmd: -exit - Выйти из приложения
";
    }

    internal class Question
    {
        public string Title { get; set; }
        public bool FileTitle { get; set; }
        public string Header { get; set; }
        public Action<string> Format { get; set; }
    }
}