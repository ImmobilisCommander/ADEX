using Adex.Common;
using Adex.Data.MetaModel;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure.Interception;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;

namespace Adex.Business
{
    public class CvsLoaderMetadata : IDisposable, ICsvLoader
    {
        public event EventHandler<MessageEventArgs> OnMessage;

        private CsvConfiguration _configuration = null;
        private CultureInfo _cultureFr = CultureInfo.CreateSpecificCulture("fr-FR");
        private Timer _timer = null;

        private HashSet<string> _existingReferences = null;
        private HashSet<string> _existingMembers = null;
        private int _mainCounter = 0;
        private bool _disposedValue;

        public CvsLoaderMetadata()
        {
            _configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = delegate (string[] tab, int count, ReadingContext ctxt)
                {
                    OnMessage?.Invoke(this, new MessageEventArgs { Message = $"Missing field found at index {count}: \"{tab[count]}\"", Level = Level.Error });
                },
                BadDataFound = delegate (ReadingContext ctxt)
                {
                    OnMessage?.Invoke(this, new MessageEventArgs { Message = ctxt.RawRecord, Level = Level.Error });
                },
                HasHeaderRecord = true,
                Encoding = Encoding.UTF8
            };

            _timer = new Timer(10000);
            _timer.Elapsed += delegate (object sender, ElapsedEventArgs e)
            {
                OnMessage?.Invoke(this, new MessageEventArgs { Message = _mainCounter.ToString().PadLeft(10, '0') });
            };
            _timer.Enabled = true;
            _timer.Start();
        }

        ~CvsLoaderMetadata()
        {
            Dispose(disposing: false);
        }

        public void LoadReferences()
        {
            using (var db = new AdexMetaContext())
            {
                _existingReferences = new HashSet<string>(db.Entities.Select(x => x.Reference));
                _existingMembers = new HashSet<string>(db.Members.Select(x => x.Name));
            }
        }

        public void LoadProviders(string path)
        {
            OnMessage?.Invoke(this, new MessageEventArgs { Message = $"Processing \"{path}\" file" });

            int counter = 0;
            using (var sr = new StreamReader(path, true))
            {
                using (var csv = new CustomCsvReader(sr, _configuration))
                {
                    csv.Read();
                    csv.ReadHeader();

                    using (var db = new AdexMetaContext())
                    {
                        db.Database.Log = delegate (string log)
                        {
                            OnMessage?.Invoke(this, new MessageEventArgs { Message = log });
                        };

                        var header = csv.Context.HeaderRecord;
                        IEnumerable<string> membersToAdd = from x in header
                                                           where !_existingMembers.Any(y => header.Contains(y))
                                                           select x;

                        if (membersToAdd != null && membersToAdd.Any())
                        {
                            foreach (var m in membersToAdd)
                            {
                                db.Members.Add(new Member { Name = m });
                            }
                            db.SaveChanges();
                        }
                    }

                    using (var db = new AdexMetaContext())
                    {
                        var members = db.Members.ToHashSet();
                        while (csv.Read())
                        {
                            var externalId = csv.GetField("identifiant");
                            if (!_existingReferences.Contains(externalId))
                            {
                                try
                                {
                                    var o = new Entity()
                                    {
                                        Reference = externalId
                                    };
                                    db.Entities.Add(o);

                                    for (int i = 0; i < csv.Context.HeaderRecord.Length; i++)
                                    {
                                        var m = new Metadata
                                        {
                                            Entity = o,
                                            Member = members.FirstOrDefault(x => x.Name == csv.Context.HeaderRecord[i]),
                                            Value = csv[i].ToString()
                                        };

                                        if (m.Member == null)
                                        {

                                        }
                                        db.Metadatas.Add(m);
                                    }

                                    _existingReferences.Add(externalId);
                                }
                                catch (Exception e)
                                {
                                    OnMessage?.Invoke(this, new MessageEventArgs { Message = $"{e.Message}: {csv.Context.RawRecord}" });
                                }
                            }

                            if (counter % 100 == 0)
                            {
                                db.SaveChanges();
                            }

                            counter++;
                            _mainCounter++;
                        }

                        db.SaveChanges();
                    }
                }
            }

            //OnMessage?.Invoke(this, new MessageEventArgs { Message = $"Found {counter} records in file \"{path}\"", Level = Level.Debug });
            //OnMessage?.Invoke(this, new MessageEventArgs { Message = $"There are {_companies.Count} new companies", Level = Level.Debug });
        }

        public void LoadLinks(string path)
        {
            throw new NotImplementedException();
        }

        public string LinksToJson()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _configuration = null;
                    _cultureFr = null;
                }

                _timer?.Dispose();
                _timer = null;

                _disposedValue = true;
            }
        }
    }
}
