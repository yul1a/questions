using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using NUnit.Framework;
using Questions;

namespace Test.Questions
{
    [TestFixture]
    public class Tests
    {
        private static readonly string inputFilePath = "input.txt";
        private static readonly string outputFilePath = "output.txt";
        private Profile profile;
        readonly FileConsole fileConsole = new FileConsole(inputFilePath, outputFilePath);

        [SetUp]
        public void Init()
        {
            var container = new DemoContainer();
            container.Register<IConsole>(delegate { return fileConsole; });

            container.Register<Profile>(delegate
            {
                return new Profile(
                    container.Create<IConsole>());
            });

            profile = container.Create<Profile>();
        }

        [TearDown]
        public void Dispose()
        {
            File.Delete(inputFilePath);
            File.Delete(outputFilePath);
            if (Directory.Exists("Анкеты"))
                Directory.Delete("Анкеты", true);
            if (Directory.Exists("zipFiles"))
                Directory.Delete("zipFiles", true);
            if (Directory.Exists("zipTemp"))
                Directory.Delete("zipTemp", true);

        }

        [Test]
        public void CreateNewProfile()
        {
            File.WriteAllLines(inputFilePath,
                new[]
                    {"-new_profile", "Горбунов Иван Александрович", "25.07.1950", "PHP", "5", "79040927025", "-save", "-exit"});

            profile.StartWork();
            Assert.True(File.Exists("Анкеты\\Горбунов Иван Александрович.txt"));
            var fileText = File.ReadAllText("Анкеты\\Горбунов Иван Александрович.txt");
            var dateFormat = DateTime.Now.ToString("dd.MM.yyyy");
            Assert.That(fileText, Is.EqualTo($@"1. ФИО: Горбунов Иван Александрович
2. Дата рождения: 25.07.1950
3. Любимый язык программирования: PHP
4. Опыт программирования на указанном языке: 5
5. Мобильный телефон: 79040927025

Анкета заполнена: {dateFormat}
"));
        }

        [Test]
        public void Statistics()
        {
            File.WriteAllLines(inputFilePath,
                new[]
                    {"-statistics", "-exit"});

            Directory.CreateDirectory("Анкеты");
            File.WriteAllText("Анкеты\\Горбунов Иван Александрович.txt",
                $@"1. ФИО: Горбунов Иван Александрович
2. Дата рождения: 25.07.2016
3. Любимый язык программирования: Java
4. Опыт программирования на указанном языке: 10
5. Мобильный телефон: 79040927025

Анкета заполнена: {DateTime.Now:dd.MM.yyyy}
");

            File.WriteAllText("Анкеты\\Иванов Иван Александрович.txt",
                $@"1. ФИО: Горбунов Иван Александрович
2. Дата рождения: 25.07.2013
3. Любимый язык программирования: PHP
4. Опыт программирования на указанном языке: 5
5. Мобильный телефон: 79040927025

Анкета заполнена: {DateTime.Now:dd.MM.yyyy}
");

            File.WriteAllText("Анкеты\\Горбунов Иван Александрович.txt",
                $@"1. ФИО: Горбунов Иван Александрович
2. Дата рождения: 25.07.2011
3. Любимый язык программирования: PHP
4. Опыт программирования на указанном языке: 5
5. Мобильный телефон: 79040927025

Анкета заполнена: {DateTime.Now:dd.MM.yyyy}
");

            profile.StartWork();

            var lines = File.ReadAllLines(outputFilePath);
            Assert.That(lines[1], Is.EqualTo("Средний возраст всех опрошенных: 4"));
            Assert.That(lines[2], Is.EqualTo("Самый популярный язык программирования: PHP"));
            Assert.That(lines[3], Is.EqualTo("Самый опытный программист: Горбунов Иван Александрович"));
        }



        [Test]
        public void List()
        {
            File.WriteAllLines(inputFilePath,
                new[]
                    {"-list", "-exit"});

            Directory.CreateDirectory("Анкеты");
            File.WriteAllText("Анкеты\\Горбунов Иван Александрович.txt",
                $@"1.");

            File.WriteAllText("Анкеты\\Иванов Иван Александрович.txt",
                $@"1.");

            profile.StartWork();

            var lines = File.ReadAllLines(outputFilePath);
            Assert.That(lines[1], Is.EqualTo("Анкеты\\Горбунов Иван Александрович.txt"));
            Assert.That(lines[2], Is.EqualTo("Анкеты\\Иванов Иван Александрович.txt"));
        }

        [Test]
        public void ListToday()
        {
            File.WriteAllLines(inputFilePath,
                new[]
                    {"-list_today", "-exit"});

            Directory.CreateDirectory("Анкеты");
            File.WriteAllText("Анкеты\\Горбунов Иван Александрович.txt",
                $@"1. ФИО: Горбунов Иван Александрович
2. Дата рождения: 25.07.2016
3. Любимый язык программирования: Java
4. Опыт программирования на указанном языке: 10
5. Мобильный телефон: 79040927025

Анкета заполнена: {DateTime.Now:dd.MM.yyyy}
");

            File.WriteAllText("Анкеты\\Иванов Федор Александрович.txt",
                $@"1. ФИО: Горбунов Иван Александрович
2. Дата рождения: 25.07.2013
3. Любимый язык программирования: PHP
4. Опыт программирования на указанном языке: 5
5. Мобильный телефон: 79040927025

Анкета заполнена: {DateTime.Today.AddDays(-10):dd.MM.yyyy}
");

            profile.StartWork();

            var lines = File.ReadAllLines(outputFilePath);
            Assert.That(lines[1], Is.EqualTo("Анкеты\\Горбунов Иван Александрович.txt"));
        }

        [Test]
        public void CreateNewProfileIncorrectExperienceFormat()
        {
            File.WriteAllLines(inputFilePath,
                new[] {"-new_profile", "Горбунов Иван Александрович", "25.07.1950", "PHP", "нет", "-exit"});

            profile.StartWork();
            Assert.False(File.Exists("Анкеты\\Горбунов Иван Александрович.txt"));
            Assert.That(File.ReadAllText(outputFilePath), Does.Contain("Неверное число - лет опыта"));
        }

        [Test]
        public void CreateNewProfileIncorrectDateFormat()
        {
            File.WriteAllLines(inputFilePath,
                new[] {"-new_profile", "Горбунов Иван Александрович", "2507.1950", "-exit"});

            profile.StartWork();
            Assert.False(File.Exists("Анкеты\\Горбунов Иван Александрович.txt"));
            Assert.That(File.ReadAllText(outputFilePath), Does.Contain("Неверный формат даты"));
        }

        [Test]
        public void CreateNewProfileIncorrectLanguage()
        {
            File.WriteAllLines(inputFilePath,
                new[] {"-new_profile", "Горбунов Иван Александрович", "25.07.1950", "никакой", "-exit"});

            profile.StartWork();
            Assert.False(File.Exists("Анкеты\\Горбунов Иван Александрович.txt"));
            Assert.That(File.ReadAllText(outputFilePath), Does.Contain("Некорректный любимый язык программирования"));
        }

        [Test]
        public void CreateNewProfile_GoToExactQuestion()
        {
            File.WriteAllLines(inputFilePath,
                new[]
                {
                    "-new_profile", "Радищев Иван Александрович", "20.01.1950", "-goto_question 1",
                    "Горбунов Иван Александрович", "25.07.1950", "PHP", "5", "79040927025", "-save", "-exit"
                });

            profile.StartWork();
            Assert.True(File.Exists("Анкеты\\Горбунов Иван Александрович.txt"));
            var fileText = File.ReadAllText("Анкеты\\Горбунов Иван Александрович.txt");
            var dateFormat = DateTime.Now.ToString("dd.MM.yyyy");
            Assert.That(fileText, Is.EqualTo($@"1. ФИО: Горбунов Иван Александрович
2. Дата рождения: 25.07.1950
3. Любимый язык программирования: PHP
4. Опыт программирования на указанном языке: 5
5. Мобильный телефон: 79040927025

Анкета заполнена: {dateFormat}
"));
        }

        [Test]
        public void CreateNewProfile_GoToPreviousQuestion()
        {
            File.WriteAllLines(inputFilePath,
                new[]
                {
                    "-new_profile", "Радищев Иван Александрович", "20.01.1950", "-goto_prev_question", "25.07.1950",
                    "PHP", "5", "79040927025", "-save", "-exit"
                });

            profile.StartWork();
            Assert.True(File.Exists("Анкеты\\Радищев Иван Александрович.txt"));
            var fileText = File.ReadAllText("Анкеты\\Радищев Иван Александрович.txt");
            var dateFormat = DateTime.Now.ToString("dd.MM.yyyy");
            Assert.That(fileText, Is.EqualTo($@"1. ФИО: Радищев Иван Александрович
2. Дата рождения: 25.07.1950
3. Любимый язык программирования: PHP
4. Опыт программирования на указанном языке: 5
5. Мобильный телефон: 79040927025

Анкета заполнена: {dateFormat}
"));
        }

        [Test]
        public void CreateNewProfile_Restart()
        {
            File.WriteAllLines(inputFilePath,
                new[]
                {
                    "-new_profile", "Радищев Иван Александрович", "20.01.1950", "-restart_profile",
                    "Радищев Иван Александрович", "25.07.1950", "PHP", "5", "79040927025", "-save", "-exit"
                });

            profile.StartWork();
            Assert.True(File.Exists("Анкеты\\Радищев Иван Александрович.txt"));
            var fileText = File.ReadAllText("Анкеты\\Радищев Иван Александрович.txt");
            var dateFormat = DateTime.Now.ToString("dd.MM.yyyy");
            Assert.That(fileText, Is.EqualTo($@"1. ФИО: Радищев Иван Александрович
2. Дата рождения: 25.07.1950
3. Любимый язык программирования: PHP
4. Опыт программирования на указанном языке: 5
5. Мобильный телефон: 79040927025

Анкета заполнена: {dateFormat}
"));
        }

        [Test]
        public void Exit()
        {
            File.WriteAllLines(inputFilePath,
                new[]
                    {"-exit"});

            profile.StartWork();
            Assert.That(File.ReadAllLines(outputFilePath).Last(), Is.EqualTo("Выход из программы"));
        }

        [Test]
        public void Help()
        {
            File.WriteAllLines(inputFilePath,
                new[]
                    {"-help", "-exit"});

            profile.StartWork();
            Assert.That(File.ReadAllLines(outputFilePath).Length, Is.EqualTo(17));
        }

        [Test]
        public void Delete()
        {
            Directory.CreateDirectory("Анкеты");
            var file = "Анкеты\\Радищев Иван Александрович.txt";
            File.WriteAllLines(file,
                new[]
                    {""});
            File.WriteAllLines(inputFilePath,
                new[]
                    {"-delete Радищев Иван Александрович.txt", "-exit"});

            profile.StartWork();
            Assert.That(File.Exists(file), Is.False);
        }

        [Test]
        public void Delete_FileNotFound_DirectiryNotExist()
        {
            File.WriteAllLines(inputFilePath,
                new[]
                    {"-delete Радищев Иван Александрович.txt", "-exit"});

            profile.StartWork();
            Assert.That(File.ReadAllText(outputFilePath), Does.Contain("Анкета не найдена"));
        }

        [Test]
        public void Delete_FileNotFound_DirectiryExist()
        {
            Directory.CreateDirectory("Анкеты");
            File.WriteAllLines(inputFilePath,
                new[]
                    {"-delete Радищев Иван Александрович.txt", "-exit"});

            profile.StartWork();
            Assert.That(File.ReadAllText(outputFilePath), Does.Contain("Анкета не найдена"));
        }


        [Test]
        public void Find()
        {
            Directory.CreateDirectory("Анкеты");
            var file = "Анкеты\\Радищев Иван Александрович.txt";
            File.WriteAllLines(file,
                new[]
                    {"текст"});
            File.WriteAllLines(inputFilePath,
                new[]
                    {"-find Радищев Иван Александрович.txt", "-exit"});

            profile.StartWork();
            Assert.That(File.ReadAllLines(outputFilePath)[1], Is.EqualTo("текст"));
        }

        [Test]
        public void FindIfNotExistDirectory()
        {
            File.WriteAllLines(inputFilePath,
                new[]
                    {"-find Радищев Иван Александрович.txt", "-exit"});

            profile.StartWork();
            Assert.That(File.ReadAllLines(outputFilePath)[1], Is.EqualTo("Анкета не найдена"));
        }

        [Test]
        public void FindIfNotExistFile()
        {
            Directory.CreateDirectory("Анкеты");
            File.WriteAllLines(inputFilePath,
                new[]
                    {"-find Радищев Иван Александрович.txt", "-exit"});

            profile.StartWork();
            Assert.That(File.ReadAllLines(outputFilePath)[1], Is.EqualTo("Анкета не найдена"));
        }

        [Test]
        public void CommandNotFound()
        {
            File.WriteAllLines(inputFilePath,
                new[]
                    {"-something", "-exit"});

            profile.StartWork();
            Assert.That(File.ReadAllText(outputFilePath), Does.Contain("Команда не найдена"));
        }

        [Test]
        public void Zip()
        {
            Directory.CreateDirectory("Анкеты");
            var file = "Анкеты\\Радищев Иван Александрович.txt";
            File.WriteAllLines(file,
                new[]
                    {"текст"});
            File.WriteAllLines(inputFilePath,
                new[]
                    {"-zip Радищев Иван Александрович.txt zipFiles", "-exit"});

            profile.StartWork();
            var output = @"zipFiles\Радищев Иван Александрович.zip";
            Assert.That(File.Exists(output));
            File.ReadAllBytes(output);
            var ziptemp = "ziptemp";
            Directory.CreateDirectory(ziptemp);
            using (ZipArchive archive = ZipFile.OpenRead(output))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    entry.ExtractToFile(Path.Combine(ziptemp, entry.FullName));
                }
            }
            Assert.That(File.ReadAllBytes(Path.Combine(ziptemp, "Радищев Иван Александрович.txt")), Is.EqualTo(File.ReadAllBytes(file)));
        }


    }
}