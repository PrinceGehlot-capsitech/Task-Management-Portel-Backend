using SignInPage.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.IO;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Text;

namespace SignInPage.Services
{
    public class SignInService
    {
        private readonly IMongoCollection<SignInDetails> _SignInCollection;
        private readonly string _key;


        public SignInService(IOptions<SignInSettings> signinsettings)
        {
            if (signinsettings == null)
            {
                throw new ArgumentNullException(nameof(signinsettings));
            }

            _key = signinsettings.Value.JwtKey;

            var mongoClient = new MongoClient(signinsettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(signinsettings.Value.DatabaseName);

            _SignInCollection = mongoDatabase.GetCollection<SignInDetails>(signinsettings.Value.SignInCollectionName);
        }

            public async Task<List<SignInDetails>> GetAsync() =>
                await _SignInCollection.Find(_ => true).ToListAsync();


        public async Task CreateAsync(SignInDetails newBasicDetails)
        {
            
            await _SignInCollection.InsertOneAsync(newBasicDetails);
        }

        public async Task<SignInDetails?> GetAsync(string id) =>
        await _SignInCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task<SignInDetails?> GetEmailAsync(string email) =>
         await _SignInCollection.Find(x => x.email == email).FirstOrDefaultAsync();  

        public string Authenticate(string email, string password)
        {
            var user = _SignInCollection.Find(x => x.email == email && x.password == password).FirstOrDefault();
            if (user == null)
                return null;

            if (string.IsNullOrEmpty(_key))
            {
                throw new InvalidOperationException("JwtKey is not set");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenkey = Encoding.ASCII.GetBytes(_key);
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new Claim[]{
            new Claim(ClaimTypes.Email, email),
        }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(tokenkey), SecurityAlgorithms.HmacSha256Signature
                )
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }
}
