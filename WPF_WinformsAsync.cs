        [...] Calling method
        
            LongOperation();
        }

        public async void LongOperation()
        {
            var result = await LongOperationAsync();

            _testButton.Text = result;
        }

        private async Task<string> LongOperationAsync()
        {
            Console.WriteLine("Fetching something async");
            await Task.Delay(5000);
            Console.WriteLine("Done!");

            return "aiadwjnadwoadwdoawojadwajowdawd";
        }
    }   
}
