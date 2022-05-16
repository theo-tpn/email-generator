using Newtonsoft.Json;
using System.Collections.Generic;

namespace Noctus.Domain.Models
{
    public class Plan
    {
        [JsonProperty("account")]
        public string Account { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("product")]
        public string Product { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("allow_unbinding")]
        public bool AllowUnbinding { get; set; }

        [JsonProperty("allow_reselling")]
        public bool AllowReselling { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("created")]
        public long Created { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("roles")]
        public List<string> Roles { get; set; }

        [JsonProperty("recurring")]
        public object Recurring { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class Metadata
    {
    }

    public class Discord
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("discriminator")]
        public string Discriminator { get; set; }

        [JsonProperty("avatar")]
        public string Avatar { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }
    }

    public class User
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("last_login")]
        public long LastLogin { get; set; }

        [JsonProperty("discord")]
        public Discord Discord { get; set; }

        [JsonProperty("created")]
        public long Created { get; set; }

        [JsonProperty("admin")]
        public bool Admin { get; set; }

        [JsonProperty("banned")]
        public bool Banned { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("photo_url")]
        public string PhotoUrl { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("discriminator")]
        public string Discriminator { get; set; }

        [JsonProperty("avatar")]
        public string Avatar { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
    }

    public class MetalabsLicense
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("unlocked")]
        public bool Unlocked { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("cancel_at")]
        public object CancelAt { get; set; }

        [JsonProperty("trial_end")]
        public object TrialEnd { get; set; }

        [JsonProperty("created")]
        public long Created { get; set; }

        [JsonProperty("account")]
        public string Account { get; set; }

        [JsonProperty("customer")]
        public string Customer { get; set; }

        [JsonProperty("subscription")]
        public object Subscription { get; set; }

        [JsonProperty("payment_method")]
        public object PaymentMethod { get; set; }

        [JsonProperty("plan")]
        public Plan Plan { get; set; }

        [JsonProperty("release")]
        public string Release { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
