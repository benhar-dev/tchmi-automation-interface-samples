namespace RecipeInformation
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Beckhoff.TwinCAT.HMI.Automation;
    using TcHmiAutomation;

    internal class Program
    {
        private static string Name => typeof(Program).Assembly.GetName().Name;

        private static void Main()
        {
            Utilities.Init(Name,
                out var vsHmi,
                out var hmiPrj,
                waitForDebugger: false,
                version: Defaults.VsVersion.AutoMode,
                isExperimental: false);

            // Beckhoff.TwinCAT.HMI.RecipeManagement.win-x64.1.12.251
            var nugetId = "Beckhoff.TwinCAT.HMI.RecipeManagement";
            var nugetVersion = "12.742.5";
            var nugetSourceName = TcHmiAutomationUtilities.NuGetRepoOfficial;

            var nugetSourceList = hmiPrj.GetNuGetSources();
            // ReSharper disable once UnusedVariable
            var nugetSourceTouse = Utilities.GetSpecificNuGetSource(nugetSourceList, nugetSourceName);

            var sourceToUse = @"C:\TwinCAT\Functions\TE2000-HMI-Engineering\Infrastructure\Packages";
            hmiPrj.AddNuGetPackage(sourceToUse, nugetId, nugetVersion);

            var srv = hmiPrj.GetServerInterface();
            var srvExtRecipe = Utilities.WaitForDomain(srv, "TcHmiRecipeManagement", waitMessage: "Wait for ENABLED domain...", includeNotPopulated: true);

            srvExtRecipe?.Enable();
            srvExtRecipe = Utilities.WaitForDomain(srv, "TcHmiRecipeManagement", waitMessage: "Wait for ENABLED domain...");

            if(srvExtRecipe != null)
            {
                Console.WriteLine("Information");
                Console.WriteLine("-----------------------------------------------");
                Console.WriteLine(" DomainName: {0}", srvExtRecipe.DomainName);
                Console.WriteLine(" ConfigVersion: {0}", srvExtRecipe.ConfigVersion);
                Console.WriteLine(" Version: {0}", srvExtRecipe.Version);
                Console.WriteLine(" Guid: {0}", srvExtRecipe.Guid);
                Console.WriteLine(" Loaded: {0}", srvExtRecipe.Loaded);
                Console.WriteLine(" State: {0}", srvExtRecipe.State);
                Console.WriteLine(" RID: {0}", srvExtRecipe.RuntimeIdentifier);
                Console.WriteLine(" Platform: {0}", srvExtRecipe.TargetPlatform);
            }

            var recipes = hmiPrj.GetRecipeInterface();

            recipes.AddRecipeFolder("Category #1");
            recipes.AddRecipeFolder("Category #2");
            recipes.AddRecipeFolder("Category #3");

            recipes.AddRecipeType(string.Empty, "BaseType", new JObject
            {
                ["options"] = new JObject { ["enabled"] = "None" },
                ["members"] = new JObject(),
                ["recipeTypeNames"] = new JArray()
            }.ToString());

            recipes.AddRecipeType(string.Empty, "BaseTypeTheSecond", null);

            recipes.AddRecipe("Category #2", "My first recipe", "{ \"values\": {}, \"recipeTypeName\": \"BaseType\" }");
            recipes.AddRecipe("Category #2", "My second recipe", new JObject
            {
                ["values"] = new JObject(),
                ["recipeTypeName"] = "BaseType"
            }.ToString(Formatting.Indented));

            recipes.AddRecipe(string.Empty, "RootRecipe", new JObject
            {
                ["values"] = new JObject(),
                ["recipeTypeName"] = "BaseType"
            }.ToString(Formatting.Indented));

            ShowRecipes(recipes.GetRecipeNames());

            recipes.AddRecipeTypeFolder("SubFolder");
            recipes.AddRecipeType("SubFolder", "TestRecipeType", null);
            recipes.AddRecipeType("SubFolder", "WillBeMovedRecipeType", null);
            recipes.MoveRecipeTypeLocation("SubFolder", string.Empty, "WillBeMovedRecipeType");

            Utilities.WaitForEnter("VisualStudio will be closed - enter any key...");

            vsHmi.CloseSolution();
            Utilities.CloseVisualStudio(vsHmi);
        }

        private static void ShowRecipes(string [] recipes)
        {
            if(recipes == null) return;
            foreach (var name in recipes)
            {
                if(string.IsNullOrEmpty(name)) continue;
                Console.WriteLine($"Recipe: {name}");
            }
        }
    }
}
