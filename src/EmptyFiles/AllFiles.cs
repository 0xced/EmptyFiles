﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace EmptyFiles
{
    public static class AllFiles
    {
        static Dictionary<string, string> aliases = new Dictionary<string, string>
        {
            {"jpeg", "jpg"},
            {"tiff", "tif"}
        };

        public static IReadOnlyDictionary<string, EmptyFile> Files
        {
            get => files;
        }

        static Dictionary<string, EmptyFile> files = new Dictionary<string, EmptyFile>();

        public static IReadOnlyDictionary<string, EmptyFile> Archives
        {
            get => archives;
        }

        static Dictionary<string, EmptyFile> archives = new Dictionary<string, EmptyFile>();

        public static IReadOnlyDictionary<string, EmptyFile> Documents
        {
            get => documents;
        }

        static Dictionary<string, EmptyFile> documents = new Dictionary<string, EmptyFile>();

        public static IReadOnlyDictionary<string, EmptyFile> Images
        {
            get => images;
        }

        static Dictionary<string, EmptyFile> images = new Dictionary<string, EmptyFile>();

        public static IReadOnlyDictionary<string, EmptyFile> Sheets
        {
            get => sheets;
        }

        static Dictionary<string, EmptyFile> sheets = new Dictionary<string, EmptyFile>();

        public static IReadOnlyDictionary<string, EmptyFile> Slides
        {
            get => slides;
        }

        static Dictionary<string, EmptyFile> slides = new Dictionary<string, EmptyFile>();

        static AllFiles()
        {
            var emptyFiles = Path.Combine(AssemblyLocation.CurrentDirectory, "EmptyFiles");
            foreach (var file in Directory.EnumerateFiles(emptyFiles, "*.*", SearchOption.AllDirectories))
            {
                var lastWriteTime = File.GetLastWriteTime(file);
                var category = GetCategory(file);
                var emptyFile = new EmptyFile(file, lastWriteTime, category);
                var extension = Extensions.GetExtension(file);
                Dictionary<string, EmptyFile>? categoryFiles = null;
                switch (category)
                {
                    case Category.Archive:
                        categoryFiles = archives;
                        break;
                    case Category.Document:
                        categoryFiles = documents;
                        break;
                    case Category.Image:
                        categoryFiles = images;
                        break;
                    case Category.Sheet:
                        categoryFiles = sheets;
                        break;
                    case Category.Slide:
                        categoryFiles = slides;
                        break;
                }

                categoryFiles![extension] = emptyFile;
                files[extension] = emptyFile;
                var alias = aliases.SingleOrDefault(x => x.Value == extension);
                if (alias.Key != null)
                {
                    categoryFiles![alias.Key] = emptyFile;
                    files[alias.Key] = emptyFile;
                }
            }
        }

        //public void UseFile(Category category, string file)
        //{
        //    var directory = Directory.GetParent(file).Name;
        //    return (Category) Enum.Parse(typeof(Category), directory, true);
        //}
        static Category GetCategory(string file)
        {
            var directory = Directory.GetParent(file).Name;
            return (Category) Enum.Parse(typeof(Category), directory, true);
        }

        public static IEnumerable<string> AllPaths
        {
            get { return files.Values.Select(x => x.Path); }
        }

        public static IReadOnlyCollection<string> AllExtensions
        {
            get { return files.Keys; }
        }

        public static IEnumerable<string> ArchivePaths
        {
            get { return archives.Values.Select(x => x.Path); }
        }

        public static IReadOnlyCollection<string> ArchiveExtensions
        {
            get { return archives.Keys; }
        }

        public static IEnumerable<string> DocumentPaths
        {
            get { return documents.Values.Select(x => x.Path); }
        }

        public static IReadOnlyCollection<string> DocumentExtensions
        {
            get { return documents.Keys; }
        }

        public static IEnumerable<string> ImagePaths
        {
            get { return images.Values.Select(x => x.Path); }
        }

        public static IReadOnlyCollection<string> ImageExtensions
        {
            get { return images.Keys; }
        }

        public static IEnumerable<string> SheetPaths
        {
            get { return sheets.Values.Select(x => x.Path); }
        }

        public static IReadOnlyCollection<string> SheetExtensions
        {
            get { return sheets.Keys; }
        }

        public static IEnumerable<string> SlidePaths
        {
            get { return slides.Values.Select(x => x.Path); }
        }

        public static IReadOnlyCollection<string> SlideExtensions
        {
            get { return slides.Keys; }
        }


        public static bool IsEmptyFile(string path)
        {
            Guard.FileExists(path, nameof(path));
            var extension = Extensions.GetExtension(path);
            if (!files.TryGetValue(extension, out var emptyFile))
            {
                return false;
            }

            return File.GetLastWriteTime(path) == emptyFile.LastWriteTime;
        }

        public static void CreateFile(string path, bool useEmptyStringForTextFiles = false)
        {
            var extension = Extensions.GetExtension(path);
            if (useEmptyStringForTextFiles &&
                Extensions.IsText(extension))
            {
                File.CreateText(path).Dispose();
                return;
            }

            File.Copy(GetPathFor(extension), path, true);
        }

        public static string GetPathFor(string extension)
        {
            Guard.AgainstNullOrEmpty(extension, nameof(extension));
            extension = Extensions.GetExtension(extension);
            if (files.TryGetValue(extension, out var emptyFile))
            {
                return emptyFile.Path;
            }

            throw new Exception($"Unknown extension: {extension}");
        }

        public static bool TryCreateFile(string path, bool useEmptyStringForTextFiles = false)
        {
            Guard.AgainstNullOrEmpty(path, nameof(path));
            var extension = Path.GetExtension(path);

            if (useEmptyStringForTextFiles &&
                Extensions.IsText(extension))
            {
                File.CreateText(path).Dispose();
                return true;
            }

            if (!TryGetPathFor(extension, out var source))
            {
                return false;
            }

            File.Copy(source, path, true);
            return true;
        }

        public static bool TryGetPathFor(string extension, [NotNullWhen(true)] out string? path)
        {
            extension = Extensions.GetExtension(extension);
            if (files.TryGetValue(extension, out var emptyFile))
            {
                path = emptyFile.Path;
                return true;
            }

            path = null;
            return false;
        }
    }
}