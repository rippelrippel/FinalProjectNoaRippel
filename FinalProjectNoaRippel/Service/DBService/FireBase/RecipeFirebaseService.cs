using FinalProjectNoaRippel.Models;
using FinalProjectNoaRippel.ViewModels;
using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProjectNoaRippel.Service.DBService.FireBase
{
    public class RecipeFirebaseService
    {
        private readonly FirebaseClient _db;
        private const string DB_URL = "https://finalprojectnoarippel-default-rtdb.europe-west1.firebasedatabase.app/";

        public RecipeFirebaseService()
        {
            _db = new FirebaseClient(DB_URL);
        }

        // מביא את ה-Key של הקטגוריה לפי שם
        private async Task<string?> GetCategoryKeyAsync(string uid, string categoryName)
        {
            var categories = await _db
                .Child("users")
                .Child(uid)
                .Child("categories")
                .OnceAsync<FoodCategoryData>();

            return categories.FirstOrDefault(c => c.Object.Name == categoryName)?.Key;
        }

        // שומר מתכון חדש
        public async Task<string?> AddRecipeAsync(string uid, string categoryName, Recipe recipe)
        {
            var categoryKey = await GetCategoryKeyAsync(uid, categoryName);
            if (categoryKey == null) return null;

            // שומר את המתכון ברשימה
            var recipeResult = await _db
                .Child("users")
                .Child(uid)
                .Child("categories")
                .Child(categoryKey)
                .Child("recipes")
                .PostAsync(new
                {
                    Name = recipe.Name,
                    ImageSource = recipe.ImageSource ?? "nophoto.jpeg"
                });

            recipe.Id = recipeResult.Key;

            // שומר את פרטי המתכון
            await _db
                .Child("users")
                .Child(uid)
                .Child("categories")
                .Child(categoryKey)
                .Child("recipeDetails")
                .Child(recipe.Name!)
                .PutAsync(new
                {
                    Name = recipe.Name,
                    ImageSource = recipe.ImageSource,
                    CategoryName = categoryName,
                    UserId = uid,
                    Ingredients = recipe.Ingredients ?? new List<string>(),
                    Instructions = recipe.Instructions ?? new List<string>(),
                    CreatedDate = recipe.CreatedDate,
                    UpdatedDate = recipe.UpdatedDate
                });

            return recipeResult.Key;
        }

        // טוען פרטי מתכון
        public async Task<Recipe?> GetRecipeAsync(string uid, string categoryName, string recipeName)
        {
            var categoryKey = await GetCategoryKeyAsync(uid, categoryName);
            if (categoryKey == null) return null;

            var details = await _db
                .Child("users")
                .Child(uid)
                .Child("categories")
                .Child(categoryKey)
                .Child("recipeDetails")
                .Child(recipeName)
                .OnceSingleAsync<Recipe>();

            return details;
        }

        // מעדכן מתכון קיים
        public async Task UpdateRecipeAsync(string uid, string categoryName, Recipe recipe)
        {
            var categoryKey = await GetCategoryKeyAsync(uid, categoryName);
            if (categoryKey == null) return;

            recipe.UpdatedDate = DateTime.Now;

            await _db
                .Child("users")
                .Child(uid)
                .Child("categories")
                .Child(categoryKey)
                .Child("recipeDetails")
                .Child(recipe.Name!)
                .PutAsync(new
                {
                    Name = recipe.Name,
                    ImageSource = recipe.ImageSource,
                    CategoryName = categoryName,
                    UserId = uid,
                    Ingredients = recipe.Ingredients ?? new List<string>(),
                    Instructions = recipe.Instructions ?? new List<string>(),
                    CreatedDate = recipe.CreatedDate,
                    UpdatedDate = recipe.UpdatedDate
                });
        }

        // מוחק מתכון
        public async Task DeleteRecipeAsync(string uid, string categoryName, string recipeKey, string recipeName)
        {
            var categoryKey = await GetCategoryKeyAsync(uid, categoryName);
            if (categoryKey == null) return;

            // מוחק מרשימת המתכונים
            await _db
                .Child("users")
                .Child(uid)
                .Child("categories")
                .Child(categoryKey)
                .Child("recipes")
                .Child(recipeKey)
                .DeleteAsync();

            // מוחק את פרטי המתכון
            await _db
                .Child("users")
                .Child(uid)
                .Child("categories")
                .Child(categoryKey)
                .Child("recipeDetails")
                .Child(recipeName)
                .DeleteAsync();
        }

        // מביא את כל המתכונים של קטגוריה
        public async Task<List<Recipe>> GetAllRecipesAsync(string uid, string categoryName)
        {
            var categoryKey = await GetCategoryKeyAsync(uid, categoryName);
            if (categoryKey == null) return new List<Recipe>();

            var recipes = await _db
                .Child("users")
                .Child(uid)
                .Child("categories")
                .Child(categoryKey)
                .Child("recipeDetails")
                .OnceAsync<Recipe>();

            return recipes.Select(r => r.Object).ToList();
        }
    }
}