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
    //שירות מרכזי לניהול מתכונים מול Firebase Realtime Database
    public class RecipeFirebaseService
    {
        private readonly FirebaseClient _db;
        private const string DB_URL = "https://finalprojectnoarippel-default-rtdb.europe-west1.firebasedatabase.app/";

        public RecipeFirebaseService()
        {
            _db = new FirebaseClient(DB_URL);
        }

        // מביא את המפתח של הקטגוריה לפי שם
        private async Task<string?> GetCategoryKeyAsync(string uid, string categoryName)
        {
            var categories = await _db
                .Child("users")
                .Child(uid)
                .Child("categories")
                .OnceAsync<FoodCategoryData>();

            return categories.FirstOrDefault(c => c.Object.Name?.Trim() == categoryName?.Trim())?.Key;
        }

        // שומר מתכון חדש
        public async Task<string?> AddRecipeAsync(string uid, string categoryName, Recipe recipe)
        {
            var categoryKey = await GetCategoryKeyAsync(uid, categoryName);
            if (categoryKey == null) return null;

            recipe.CategoryName = categoryName;
            recipe.CreatedDate = DateTime.Now;
            recipe.UpdatedDate = DateTime.Now;

            // שומר הכל במקום אחד
            var result = await _db
                .Child("users")
                .Child(uid)
                .Child("categories")
                .Child(categoryKey)
                .Child("recipes")
                .PostAsync(recipe);

            // שומר את ה-Id שנוצר חזרה לתוך הרשומה
            recipe.Id = result.Key;
            await _db
                .Child("users")
                .Child(uid)
                .Child("categories")
                .Child(categoryKey)
                .Child("recipes")
                .Child(result.Key)
                .PutAsync(recipe);

            return result.Key;
        }

        // טוען פרטי מתכון לפי שם
        public async Task<Recipe?> GetRecipeAsync(string uid, string categoryName, string recipeName)
        {
            var categoryKey = await GetCategoryKeyAsync(uid, categoryName);
            if (categoryKey == null) return null;

            var recipes = await _db
                .Child("users")
                .Child(uid)
                .Child("categories")
                .Child(categoryKey)
                .Child("recipes")
                .OnceAsync<Recipe>();

            return recipes.FirstOrDefault(r => r.Object.Name == recipeName)?.Object;
        }

        // מעדכן מתכון קיים לפי recipeKey
        public async Task UpdateRecipeAsync(string uid, string categoryName, Recipe recipe)
        {
            var categoryKey = await GetCategoryKeyAsync(uid, categoryName);
            if (categoryKey == null || recipe.Id == null) return;

            recipe.UpdatedDate = DateTime.Now;

            await _db
                .Child("users")
                .Child(uid)
                .Child("categories")
                .Child(categoryKey)
                .Child("recipes")
                .Child(recipe.Id)
                .PutAsync(recipe);
        }

        // מוחק מתכון לפי recipeKey
        public async Task DeleteRecipeAsync(string uid, string categoryName, string recipeKey)
        {
            var categoryKey = await GetCategoryKeyAsync(uid, categoryName);
            if (categoryKey == null) return;

            await _db
                .Child("users")
                .Child(uid)
                .Child("categories")
                .Child(categoryKey)
                .Child("recipes")
                .Child(recipeKey)
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
                .Child("recipes")
                .OnceAsync<Recipe>();

            return recipes.Select(r => r.Object).ToList();
        }
    }
}