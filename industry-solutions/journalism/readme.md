#         [TestInitialize()]
        public void Startup()
        {
            var modelList = new List<int>();

            foreach (var module in (ModuleEnumeration[])Enum.GetValues(typeof(ModuleEnumeration)))
            {
                modelList.Add((int)module);
            }

            AllModules.Init(modelList);
        }
