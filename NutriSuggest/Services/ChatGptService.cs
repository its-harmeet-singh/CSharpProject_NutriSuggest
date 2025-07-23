using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using NutriSuggest.Models;

namespace NutriSuggest.Services
{
    public class ChatGptService
    {
        private readonly ChatClient _chatClient;

        public ChatGptService(IConfiguration config)
        {
            var apiKey = config["OpenAI:ApiKey"]
                         ?? throw new ArgumentNullException("OpenAI:ApiKey");
            var model = config["OpenAI:Model"]
                         ?? throw new ArgumentNullException("OpenAI:Model");

            var client = new OpenAIClient(apiKey);
            _chatClient = client.GetChatClient(model);
        }

        public async Task<List<RecipeSuggestion>> SuggestRecipesAsync(
            List<string> ingredients,
            bool vegetarian,
            bool glutenFree)
        {

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(
                    "You are a cooking assistant that responds ONLY with a JSON array.  " +
                    "Do NOT output any explanatory text—just the raw JSON array."
                ),
                new UserChatMessage(
                    $"Ingredients: {string.Join(", ", ingredients)}. " +
                    $"Vegetarian: {(vegetarian ? "yes" : "no")}, " +
                    $"Gluten-free: {(glutenFree ? "yes" : "no")}. " +
                    "Return ONLY JSON. Each recipe must have:" +
                    "\n• Title: a string" +
                    "\n• Ingredients: an ARRAY of strings" +
                    "\n• Instructions: an ARRAY of strings (one step per entry)."
                )

            };

            ChatCompletion completion = await _chatClient.CompleteChatAsync(messages);
            string raw = completion.Content[0].Text ?? "";

            int start = raw.IndexOf('[');
            int end = raw.LastIndexOf(']') + 1;
            string json = (start >= 0 && end > start)
                ? raw[start..end]
                : raw.Trim();

            return JsonSerializer.Deserialize<List<RecipeSuggestion>>(json)
                   ?? new List<RecipeSuggestion>();
        }

        public async Task<List<string>> SuggestSubstitutesAsync(
    string ingredient,
    bool vegetarian,
    bool glutenFree)
        {
            // 1) Build your messages
            var messages = new List<ChatMessage>
    {
        new SystemChatMessage(
            "You are a cooking assistant that responds ONLY with a JSON array.  " +
            "Do NOT output any explanatory text—just the raw JSON array."
        ),
        new UserChatMessage(
            $"Ingredient: {ingredient}. " +
            $"Vegetarian: {(vegetarian ? "yes" : "no")}, " +
            $"Gluten-free: {(glutenFree ? "yes" : "no")}. " +
            "Return ONLY JSON: an array of up to 5 substitute ingredients as strings."
        )
    };

            // 2) Send to OpenAI
            ChatCompletion completion = await _chatClient.CompleteChatAsync(messages);

            // 3) Extract the assistant’s reply exactly as before
            string raw = completion.Content[0].Text?.Trim() ?? "";

            // 4) Snip the JSON array out
            int start = raw.IndexOf('[');
            int end = raw.LastIndexOf(']') + 1;
            string json = (start >= 0 && end > start)
                ? raw[start..end]
                : raw;

            // 5) Deserialize
            return JsonSerializer.Deserialize<List<string>>(json)
                   ?? new List<string>();
        }


    }
}
