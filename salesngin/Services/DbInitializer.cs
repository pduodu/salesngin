// Ignore Spelling: admin
using Microsoft.EntityFrameworkCore;
namespace salesngin.Services;

public static class DbInitializer
{
    public static async Task Initialize(IApplicationBuilder app)
    {
        const string superAdminEmail = "pduodu60@gmail.com";
        const string adminEmail = "admin@mail.com";
        const string staffEmail = "staff@mail.com";
        const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()-_=+";

        using var serviceScope = app.ApplicationServices.CreateScope();

        var _databaseContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var _userManager = serviceScope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
        var _roleManager = serviceScope.ServiceProvider.GetService<RoleManager<ApplicationRole>>();
        var _mailService = serviceScope.ServiceProvider.GetService<IMailService>();
        var _webHostEnvironment = serviceScope.ServiceProvider.GetService<IWebHostEnvironment>();

        //Ensure that the database is created
        _databaseContext.Database.EnsureCreated();
        // Apply pending migrations
        //_databaseContext.Database.Migrate();

        //Settings 
        if (!await _databaseContext.ApplicationSettings.AnyAsync())
        {
            ApplicationSetting settings = new()
            {
                ReceiptMessage = "",
                ReceiptAdvertA = "",
                ReceiptAdvertB = "",
                MaxStockLevelFactor = 2.0M
            };
            _databaseContext.ApplicationSettings.Add(settings);
            _databaseContext.SaveChanges();
        }

        //Seed Units
        if (!await _databaseContext.Units.AnyAsync())
        {
            List<Unit> unitList =
            [
                new Unit() { UnitName = "Owner" },
                    new Unit() { UnitName = "Sales" },
                    new Unit() { UnitName = "IT" },
                    new Unit() { UnitName = "Facility" },
                    new Unit() { UnitName = "Audit" },
                    new Unit() { UnitName = "Marketing" },
                ];
            await _databaseContext.AddRangeAsync(unitList);
            await _databaseContext.SaveChangesAsync();
        }

        //Seed Categories
        // if (!await _databaseContext.Categories.AnyAsync())
        // {
        //     List<Category> categoryList = [];
        //     foreach (var category in GlobalConstants.DefaultCategories)
        //     {
        //         Category newCategory = new() { ParentId = null, CategoryName = category.Item2  };
        //         categoryList.Add(newCategory);
        //     }

        //     if (categoryList.Count > 0)
        //     {
        //         await _databaseContext.AddRangeAsync(categoryList);
        //         await _databaseContext.SaveChangesAsync();
        //     }
        // }


        //Roles
        if (!await _roleManager.Roles.AnyAsync())
        {
            List<ApplicationRole> roles = [
                new ApplicationRole() { NormalizedName=ApplicationRoles.SuperAdministrator.ToUpper(), Name = ApplicationRoles.SuperAdministrator, RoleDisplayText = "System Administrator", RoleDescription="Best for system administrators" },
                    new ApplicationRole() { NormalizedName=ApplicationRoles.Administrator.ToUpper(), Name = ApplicationRoles.Administrator, RoleDisplayText = "Administrator", RoleDescription="Best for users who mange staff records" },
                    new ApplicationRole() { NormalizedName=ApplicationRoles.Staff.ToUpper(), Name = ApplicationRoles.Staff, RoleDisplayText = "Staff", RoleDescription="Best for general staff" },
                    new ApplicationRole() { NormalizedName=ApplicationRoles.ViewOnly.ToUpper(), Name = ApplicationRoles.ViewOnly, RoleDisplayText = "View Only User", RoleDescription="Best for auditors and inspectors" },
                ];
            _databaseContext.Roles.AddRange(roles);
            _databaseContext.SaveChanges();
        }

        //Countries
        if (!await _databaseContext.Countries.AnyAsync())
        {
            List<Country> countries =
            [
                new Country() { AlphaCode2 = "AF", AlphaCode3 = "AFG", CountryName = "Afghanistan", Nationality = "Afghan" },
                    new Country() { AlphaCode2 = "AX", AlphaCode3 = "ALA", CountryName = "Åland Islands", Nationality = "Åland Island" },
                    new Country() { AlphaCode2 = "AL", AlphaCode3 = "ALB", CountryName = "Albania", Nationality = "Albanian" },
                    new Country() { AlphaCode2 = "DZ", AlphaCode3 = "DZA", CountryName = "Algeria", Nationality = "Algerian" },
                    new Country() { AlphaCode2 = "AS", AlphaCode3 = "ASM", CountryName = "American Samoa", Nationality = "American Samoan" },
                    new Country() { AlphaCode2 = "AD", AlphaCode3 = "AND", CountryName = "Andorra", Nationality = "Andorran" },
                    new Country() { AlphaCode2 = "AO", AlphaCode3 = "AGO", CountryName = "Angola", Nationality = "Angolan" },
                    new Country() { AlphaCode2 = "AI", AlphaCode3 = "AIA", CountryName = "Anguilla", Nationality = "Anguillan" },
                    new Country() { AlphaCode2 = "AQ", AlphaCode3 = "ATA", CountryName = "Antarctica", Nationality = "Antarctic" },
                    new Country() { AlphaCode2 = "AG", AlphaCode3 = "ATG", CountryName = "Antigua and Barbuda", Nationality = "Antiguan or Barbudan" },
                    new Country() { AlphaCode2 = "AR", AlphaCode3 = "ARG", CountryName = "Argentina", Nationality = "Argentine" },
                    new Country() { AlphaCode2 = "AM", AlphaCode3 = "ARM", CountryName = "Armenia", Nationality = "Armenian" },
                    new Country() { AlphaCode2 = "AW", AlphaCode3 = "ABW", CountryName = "Aruba", Nationality = "Aruban" },
                    new Country() { AlphaCode2 = "AU", AlphaCode3 = "AUS", CountryName = "Australia", Nationality = "Australian" },
                    new Country() { AlphaCode2 = "AT", AlphaCode3 = "AUT", CountryName = "Austria", Nationality = "Austrian" },
                    new Country() { AlphaCode2 = "AZ", AlphaCode3 = "AZE", CountryName = "Azerbaijan", Nationality = "Azerbaijani, Azeri" },
                    new Country() { AlphaCode2 = "BS", AlphaCode3 = "BHS", CountryName = "Bahamas", Nationality = "Bahamian" },
                    new Country() { AlphaCode2 = "BH", AlphaCode3 = "BHR", CountryName = "Bahrain", Nationality = "Bahraini" },
                    new Country() { AlphaCode2 = "BD", AlphaCode3 = "BGD", CountryName = "Bangladesh", Nationality = "Bangladeshi" },
                    new Country() { AlphaCode2 = "BB", AlphaCode3 = "BRB", CountryName = "Barbados", Nationality = "Barbadian" },
                    new Country() { AlphaCode2 = "BY", AlphaCode3 = "BLR", CountryName = "Belarus", Nationality = "Belarusian" },
                    new Country() { AlphaCode2 = "BE", AlphaCode3 = "BEL", CountryName = "Belgium", Nationality = "Belgian" },
                    new Country() { AlphaCode2 = "BZ", AlphaCode3 = "BLZ", CountryName = "Belize", Nationality = "Belizean" },
                    new Country() { AlphaCode2 = "BJ", AlphaCode3 = "BEN", CountryName = "Benin", Nationality = "Beninois" },
                    new Country() { AlphaCode2 = "BM", AlphaCode3 = "BMU", CountryName = "Bermuda", Nationality = "Bermudian" },
                    new Country() { AlphaCode2 = "BT", AlphaCode3 = "BTN", CountryName = "Bhutan", Nationality = "Bhutanese" },
                    new Country() { AlphaCode2 = "BO", AlphaCode3 = "BOL", CountryName = "Bolivia", Nationality = "Bolivian" },
                    new Country() { AlphaCode2 = "BQ", AlphaCode3 = "BES", CountryName = "Bonaire", Nationality = "Bonaire" },
                    new Country() { AlphaCode2 = "BA", AlphaCode3 = "BIH", CountryName = "Bosnia and Herzegovina", Nationality = "Bosnian or Herzegovinian" },
                    new Country() { AlphaCode2 = "BW", AlphaCode3 = "BWA", CountryName = "Botswana", Nationality = "Motswana" },
                    new Country() { AlphaCode2 = "BV", AlphaCode3 = "BVT", CountryName = "Bouvet Island", Nationality = "Bouvet Island" },
                    new Country() { AlphaCode2 = "BR", AlphaCode3 = "BRA", CountryName = "Brazil", Nationality = "Brazilian" },
                    new Country() { AlphaCode2 = "IO", AlphaCode3 = "IOT", CountryName = "British Indian Ocean Territory", Nationality = "BIOT" },
                    new Country() { AlphaCode2 = "BN", AlphaCode3 = "BRN", CountryName = "Brunei Darussalam", Nationality = "Bruneian" },
                    new Country() { AlphaCode2 = "BG", AlphaCode3 = "BGR", CountryName = "Bulgaria", Nationality = "Bulgarian" },
                    new Country() { AlphaCode2 = "BF", AlphaCode3 = "BFA", CountryName = "Burkina Faso", Nationality = "Burkinabé" },
                    new Country() { AlphaCode2 = "BI", AlphaCode3 = "BDI", CountryName = "Burundi", Nationality = "Burundian" },
                    new Country() { AlphaCode2 = "CV", AlphaCode3 = "CPV", CountryName = "Cabo Verde", Nationality = "Cabo Verdean" },
                    new Country() { AlphaCode2 = "KH", AlphaCode3 = "KHM", CountryName = "Cambodia", Nationality = "Cambodian" },
                    new Country() { AlphaCode2 = "CM", AlphaCode3 = "CMR", CountryName = "Cameroon", Nationality = "Cameroonian" },
                    new Country() { AlphaCode2 = "CA", AlphaCode3 = "CAN", CountryName = "Canada", Nationality = "Canadian" },
                    new Country() { AlphaCode2 = "KY", AlphaCode3 = "CYM", CountryName = "Cayman Islands", Nationality = "Caymanian" },
                    new Country() { AlphaCode2 = "CF", AlphaCode3 = "CAF", CountryName = "Central African Republic", Nationality = "Central African" },
                    new Country() { AlphaCode2 = "TD", AlphaCode3 = "TCD", CountryName = "Chad", Nationality = "Chadian" },
                    new Country() { AlphaCode2 = "CL", AlphaCode3 = "CHL", CountryName = "Chile", Nationality = "Chilean" },
                    new Country() { AlphaCode2 = "CN", AlphaCode3 = "CHN", CountryName = "China", Nationality = "Chinese" },
                    new Country() { AlphaCode2 = "CX", AlphaCode3 = "CXR", CountryName = "Christmas Island", Nationality = "Christmas Island" },
                    new Country() { AlphaCode2 = "CC", AlphaCode3 = "CCK", CountryName = "Cocos Islands", Nationality = "Cocos Island" },
                    new Country() { AlphaCode2 = "CO", AlphaCode3 = "COL", CountryName = "Colombia", Nationality = "Colombian" },
                    new Country() { AlphaCode2 = "KM", AlphaCode3 = "COM", CountryName = "Comoros", Nationality = "Comoran" },
                    new Country() { AlphaCode2 = "CG", AlphaCode3 = "COG", CountryName = "Congo", Nationality = "Congolese" },
                    new Country() { AlphaCode2 = "CD", AlphaCode3 = "COD", CountryName = "Congo DR", Nationality = "Congolese" },
                    new Country() { AlphaCode2 = "CK", AlphaCode3 = "COK", CountryName = "Cook Islands", Nationality = "Cook Island" },
                    new Country() { AlphaCode2 = "CR", AlphaCode3 = "CRI", CountryName = "Costa Rica", Nationality = "Costa Rican" },
                    new Country() { AlphaCode2 = "CI", AlphaCode3 = "CIV", CountryName = "CôteIvoire", Nationality = "Ivorian" },
                    new Country() { AlphaCode2 = "HR", AlphaCode3 = "HRV", CountryName = "Croatia", Nationality = "Croatian" },
                    new Country() { AlphaCode2 = "CU", AlphaCode3 = "CUB", CountryName = "Cuba", Nationality = "Cuban" },
                    new Country() { AlphaCode2 = "CW", AlphaCode3 = "CUW", CountryName = "Curaçao", Nationality = "Curaçaoan" },
                    new Country() { AlphaCode2 = "CY", AlphaCode3 = "CYP", CountryName = "Cyprus", Nationality = "Cypriot" },
                    new Country() { AlphaCode2 = "CZ", AlphaCode3 = "CZE", CountryName = "Czech Republic", Nationality = "Czech" },
                    new Country() { AlphaCode2 = "DK", AlphaCode3 = "DNK", CountryName = "Denmark", Nationality = "Danish" },
                    new Country() { AlphaCode2 = "DJ", AlphaCode3 = "DJI", CountryName = "Djibouti", Nationality = "Djiboutian" },
                    new Country() { AlphaCode2 = "DM", AlphaCode3 = "DMA", CountryName = "Dominica", Nationality = "Dominican" },
                    new Country() { AlphaCode2 = "DO", AlphaCode3 = "DOM", CountryName = "Dominican Republic", Nationality = "Dominican" },
                    new Country() { AlphaCode2 = "EC", AlphaCode3 = "ECU", CountryName = "Ecuador", Nationality = "Ecuadorian" },
                    new Country() { AlphaCode2 = "EG", AlphaCode3 = "EGY", CountryName = "Egypt", Nationality = "Egyptian" },
                    new Country() { AlphaCode2 = "SV", AlphaCode3 = "SLV", CountryName = "El Salvador", Nationality = "Salvadoran" },
                    new Country() { AlphaCode2 = "GQ", AlphaCode3 = "GNQ", CountryName = "Equatorial Guinea", Nationality = "Equatorial Guinean" },
                    new Country() { AlphaCode2 = "ER", AlphaCode3 = "ERI", CountryName = "Eritrea", Nationality = "Eritrean" },
                    new Country() { AlphaCode2 = "EE", AlphaCode3 = "EST", CountryName = "Estonia", Nationality = "Estonian" },
                    new Country() { AlphaCode2 = "ET", AlphaCode3 = "ETH", CountryName = "Ethiopia", Nationality = "Ethiopian" },
                    new Country() { AlphaCode2 = "FK", AlphaCode3 = "FLK", CountryName = "Falkland Islands", Nationality = "Falkland Island" },
                    new Country() { AlphaCode2 = "FO", AlphaCode3 = "FRO", CountryName = "Faroe Islands", Nationality = "Faroese" },
                    new Country() { AlphaCode2 = "FJ", AlphaCode3 = "FJI", CountryName = "Fiji", Nationality = "Fijian" },
                    new Country() { AlphaCode2 = "FI", AlphaCode3 = "FIN", CountryName = "Finland", Nationality = "Finnish" },
                    new Country() { AlphaCode2 = "FR", AlphaCode3 = "FRA", CountryName = "France", Nationality = "French" },
                    new Country() { AlphaCode2 = "GF", AlphaCode3 = "GUF", CountryName = "French Guiana", Nationality = "French Guianese" },
                    new Country() { AlphaCode2 = "PF", AlphaCode3 = "PYF", CountryName = "French Polynesia", Nationality = "French Polynesian" },
                    new Country() { AlphaCode2 = "TF", AlphaCode3 = "ATF", CountryName = "French Southern Territories", Nationality = "French Southern Territories" },
                    new Country() { AlphaCode2 = "GA", AlphaCode3 = "GAB", CountryName = "Gabon", Nationality = "Gabonese" },
                    new Country() { AlphaCode2 = "GM", AlphaCode3 = "GMB", CountryName = "Gambia", Nationality = "Gambian" },
                    new Country() { AlphaCode2 = "GE", AlphaCode3 = "GEO", CountryName = "Georgia", Nationality = "Georgian" },
                    new Country() { AlphaCode2 = "DE", AlphaCode3 = "DEU", CountryName = "Germany", Nationality = "German" },
                    new Country() { AlphaCode2 = "GH", AlphaCode3 = "GHA", CountryName = "Ghana", Nationality = "Ghanaian" },
                    new Country() { AlphaCode2 = "GI", AlphaCode3 = "GIB", CountryName = "Gibraltar", Nationality = "Gibraltar" },
                    new Country() { AlphaCode2 = "GR", AlphaCode3 = "GRC", CountryName = "Greece", Nationality = "Greek, Hellenic" },
                    new Country() { AlphaCode2 = "GL", AlphaCode3 = "GRL", CountryName = "Greenland", Nationality = "Greenlandic" },
                    new Country() { AlphaCode2 = "GD", AlphaCode3 = "GRD", CountryName = "Grenada", Nationality = "Grenadian" },
                    new Country() { AlphaCode2 = "GP", AlphaCode3 = "GLP", CountryName = "Guadeloupe", Nationality = "Guadeloupe" },
                    new Country() { AlphaCode2 = "GU", AlphaCode3 = "GUM", CountryName = "Guam", Nationality = "Guamanian, Guambat" },
                    new Country() { AlphaCode2 = "GT", AlphaCode3 = "GTM", CountryName = "Guatemala", Nationality = "Guatemalan" },
                    new Country() { AlphaCode2 = "GG", AlphaCode3 = "GGY", CountryName = "Guernsey", Nationality = "Channel Island" },
                    new Country() { AlphaCode2 = "GN", AlphaCode3 = "GIN", CountryName = "Guinea", Nationality = "Guinean" },
                    new Country() { AlphaCode2 = "GW", AlphaCode3 = "GNB", CountryName = "Guinea-Bissau", Nationality = "Bissau-Guinean" },
                    new Country() { AlphaCode2 = "GY", AlphaCode3 = "GUY", CountryName = "Guyana", Nationality = "Guyanese" },
                    new Country() { AlphaCode2 = "HT", AlphaCode3 = "HTI", CountryName = "Haiti", Nationality = "Haitian" },
                    new Country() { AlphaCode2 = "HM", AlphaCode3 = "HMD", CountryName = "Heard Island and McDonald Islands", Nationality = "Heard Island or McDonald Islands" },
                    new Country() { AlphaCode2 = "VA", AlphaCode3 = "VAT", CountryName = "Vatican City State", Nationality = "Vatican" },
                    new Country() { AlphaCode2 = "HN", AlphaCode3 = "HND", CountryName = "Honduras", Nationality = "Honduran" },
                    new Country() { AlphaCode2 = "HK", AlphaCode3 = "HKG", CountryName = "Hong Kong", Nationality = "Hong Kong, Hong Kongese" },
                    new Country() { AlphaCode2 = "HU", AlphaCode3 = "HUN", CountryName = "Hungary", Nationality = "Hungarian, Magyar" },
                    new Country() { AlphaCode2 = "IS", AlphaCode3 = "ISL", CountryName = "Iceland", Nationality = "Icelandic" },
                    new Country() { AlphaCode2 = "IN", AlphaCode3 = "IND", CountryName = "India", Nationality = "Indian" },
                    new Country() { AlphaCode2 = "ID", AlphaCode3 = "IDN", CountryName = "Indonesia", Nationality = "Indonesian" },
                    new Country() { AlphaCode2 = "IR", AlphaCode3 = "IRN", CountryName = "Iran", Nationality = "Iranian, Persian" },
                    new Country() { AlphaCode2 = "IQ", AlphaCode3 = "IRQ", CountryName = "Iraq", Nationality = "Iraqi" },
                    new Country() { AlphaCode2 = "IE", AlphaCode3 = "IRL", CountryName = "Ireland", Nationality = "Irish" },
                    new Country() { AlphaCode2 = "IM", AlphaCode3 = "IMN", CountryName = "Isle of Man", Nationality = "Manx" },
                    new Country() { AlphaCode2 = "IL", AlphaCode3 = "ISR", CountryName = "Israel", Nationality = "Israeli" },
                    new Country() { AlphaCode2 = "IT", AlphaCode3 = "ITA", CountryName = "Italy", Nationality = "Italian" },
                    new Country() { AlphaCode2 = "JM", AlphaCode3 = "JAM", CountryName = "Jamaica", Nationality = "Jamaican" },
                    new Country() { AlphaCode2 = "JP", AlphaCode3 = "JPN", CountryName = "Japan", Nationality = "Japanese" },
                    new Country() { AlphaCode2 = "JE", AlphaCode3 = "JEY", CountryName = "Jersey", Nationality = "Channel Island" },
                    new Country() { AlphaCode2 = "JO", AlphaCode3 = "JOR", CountryName = "Jordan", Nationality = "Jordanian" },
                    new Country() { AlphaCode2 = "KZ", AlphaCode3 = "KAZ", CountryName = "Kazakhstan", Nationality = "Kazakhstani, Kazakh" },
                    new Country() { AlphaCode2 = "KE", AlphaCode3 = "KEN", CountryName = "Kenya", Nationality = "Kenyan" },
                    new Country() { AlphaCode2 = "KI", AlphaCode3 = "KIR", CountryName = "Kiribati", Nationality = "I-Kiribati" },
                    new Country() { AlphaCode2 = "KP", AlphaCode3 = "PRK", CountryName = "North Korea", Nationality = "North Korean" },
                    new Country() { AlphaCode2 = "KR", AlphaCode3 = "KOR", CountryName = "South Korea", Nationality = "South Korean" },
                    new Country() { AlphaCode2 = "KW", AlphaCode3 = "KWT", CountryName = "Kuwait", Nationality = "Kuwaiti" },
                    new Country() { AlphaCode2 = "KG", AlphaCode3 = "KGZ", CountryName = "Kyrgyzstan", Nationality = "Kyrgyzstani" },
                    new Country() { AlphaCode2 = "LA", AlphaCode3 = "LAO", CountryName = "Lao", Nationality = "Laotian" },
                    new Country() { AlphaCode2 = "LV", AlphaCode3 = "LVA", CountryName = "Latvia", Nationality = "Latvian" },
                    new Country() { AlphaCode2 = "LB", AlphaCode3 = "LBN", CountryName = "Lebanon", Nationality = "Lebanese" },
                    new Country() { AlphaCode2 = "LS", AlphaCode3 = "LSO", CountryName = "Lesotho", Nationality = "Basotho" },
                    new Country() { AlphaCode2 = "LR", AlphaCode3 = "LBR", CountryName = "Liberia", Nationality = "Liberian" },
                    new Country() { AlphaCode2 = "LY", AlphaCode3 = "LBY", CountryName = "Libya", Nationality = "Libyan" },
                    new Country() { AlphaCode2 = "LI", AlphaCode3 = "LIE", CountryName = "Liechtenstein", Nationality = "Liechtenstein" },
                    new Country() { AlphaCode2 = "LT", AlphaCode3 = "LTU", CountryName = "Lithuania", Nationality = "Lithuanian" },
                    new Country() { AlphaCode2 = "LU", AlphaCode3 = "LUX", CountryName = "Luxembourg", Nationality = "Luxembourg, Luxembourgish" },
                    new Country() { AlphaCode2 = "MO", AlphaCode3 = "MAC", CountryName = "Macao", Nationality = "Macanese, Chinese" },
                    new Country() { AlphaCode2 = "MK", AlphaCode3 = "MKD", CountryName = "Macedonia", Nationality = "Macedonian" },
                    new Country() { AlphaCode2 = "MG", AlphaCode3 = "MDG", CountryName = "Madagascar", Nationality = "Malagasy" },
                    new Country() { AlphaCode2 = "MW", AlphaCode3 = "MWI", CountryName = "Malawi", Nationality = "Malawian" },
                    new Country() { AlphaCode2 = "MY", AlphaCode3 = "MYS", CountryName = "Malaysia", Nationality = "Malaysian" },
                    new Country() { AlphaCode2 = "MV", AlphaCode3 = "MDV", CountryName = "Maldives", Nationality = "Maldivian" },
                    new Country() { AlphaCode2 = "ML", AlphaCode3 = "MLI", CountryName = "Mali", Nationality = "Malian, Malinese" },
                    new Country() { AlphaCode2 = "MT", AlphaCode3 = "MLT", CountryName = "Malta", Nationality = "Maltese" },
                    new Country() { AlphaCode2 = "MH", AlphaCode3 = "MHL", CountryName = "Marshall Islands", Nationality = "Marshallese" },
                    new Country() { AlphaCode2 = "MQ", AlphaCode3 = "MTQ", CountryName = "Martinique", Nationality = "Martiniquais, Martinican" },
                    new Country() { AlphaCode2 = "MR", AlphaCode3 = "MRT", CountryName = "Mauritania", Nationality = "Mauritanian" },
                    new Country() { AlphaCode2 = "MU", AlphaCode3 = "MUS", CountryName = "Mauritius", Nationality = "Mauritian" },
                    new Country() { AlphaCode2 = "YT", AlphaCode3 = "MYT", CountryName = "Mayotte", Nationality = "Mahoran" },
                    new Country() { AlphaCode2 = "MX", AlphaCode3 = "MEX", CountryName = "Mexico", Nationality = "Mexican" },
                    new Country() { AlphaCode2 = "FM", AlphaCode3 = "FSM", CountryName = "Micronesia", Nationality = "Micronesian" },
                    new Country() { AlphaCode2 = "MD", AlphaCode3 = "MDA", CountryName = "Moldova", Nationality = "Moldovan" },
                    new Country() { AlphaCode2 = "MC", AlphaCode3 = "MCO", CountryName = "Monaco", Nationality = "Monacan" },
                    new Country() { AlphaCode2 = "MN", AlphaCode3 = "MNG", CountryName = "Mongolia", Nationality = "Mongolian" },
                    new Country() { AlphaCode2 = "ME", AlphaCode3 = "MNE", CountryName = "Montenegro", Nationality = "Montenegrin" },
                    new Country() { AlphaCode2 = "MS", AlphaCode3 = "MSR", CountryName = "Montserrat", Nationality = "Montserratian" },
                    new Country() { AlphaCode2 = "MA", AlphaCode3 = "MAR", CountryName = "Morocco", Nationality = "Moroccan" },
                    new Country() { AlphaCode2 = "MZ", AlphaCode3 = "MOZ", CountryName = "Mozambique", Nationality = "Mozambican" },
                    new Country() { AlphaCode2 = "MM", AlphaCode3 = "MMR", CountryName = "Myanmar", Nationality = "Burmese" },
                    new Country() { AlphaCode2 = "NA", AlphaCode3 = "NAM", CountryName = "Namibia", Nationality = "Namibian" },
                    new Country() { AlphaCode2 = "NR", AlphaCode3 = "NRU", CountryName = "Nauru", Nationality = "Nauruan" },
                    new Country() { AlphaCode2 = "NP", AlphaCode3 = "NPL", CountryName = "Nepal", Nationality = "Nepali, Nepalese" },
                    new Country() { AlphaCode2 = "NL", AlphaCode3 = "NLD", CountryName = "Netherlands", Nationality = "Dutch" },
                    new Country() { AlphaCode2 = "NC", AlphaCode3 = "NCL", CountryName = "New Caledonia", Nationality = "New Caledonian" },
                    new Country() { AlphaCode2 = "NZ", AlphaCode3 = "NZL", CountryName = "New Zealand", Nationality = "New Zealander" },
                    new Country() { AlphaCode2 = "NI", AlphaCode3 = "NIC", CountryName = "Nicaragua", Nationality = "Nicaraguan" },
                    new Country() { AlphaCode2 = "NE", AlphaCode3 = "NER", CountryName = "Niger", Nationality = "Nigerien" },
                    new Country() { AlphaCode2 = "NG", AlphaCode3 = "NGA", CountryName = "Nigeria", Nationality = "Nigerian" },
                    new Country() { AlphaCode2 = "NU", AlphaCode3 = "NIU", CountryName = "Niue", Nationality = "Niuean" },
                    new Country() { AlphaCode2 = "NF", AlphaCode3 = "NFK", CountryName = "Norfolk Island", Nationality = "Norfolk Island" },
                    new Country() { AlphaCode2 = "MP", AlphaCode3 = "MNP", CountryName = "Northern Mariana Islands", Nationality = "Northern Marianan" },
                    new Country() { AlphaCode2 = "NO", AlphaCode3 = "NOR", CountryName = "Norway", Nationality = "Norwegian" },
                    new Country() { AlphaCode2 = "OM", AlphaCode3 = "OMN", CountryName = "Oman", Nationality = "Omani" },
                    new Country() { AlphaCode2 = "PK", AlphaCode3 = "PAK", CountryName = "Pakistan", Nationality = "Pakistani" },
                    new Country() { AlphaCode2 = "PW", AlphaCode3 = "PLW", CountryName = "Palau", Nationality = "Palauan" },
                    new Country() { AlphaCode2 = "PS", AlphaCode3 = "PSE", CountryName = "Palestine", Nationality = "Palestinian" },
                    new Country() { AlphaCode2 = "PA", AlphaCode3 = "PAN", CountryName = "Panama", Nationality = "Panamanian" },
                    new Country() { AlphaCode2 = "PG", AlphaCode3 = "PNG", CountryName = "Papua New Guinea", Nationality = "Papua New Guinean" },
                    new Country() { AlphaCode2 = "PY", AlphaCode3 = "PRY", CountryName = "Paraguay", Nationality = "Paraguayan" },
                    new Country() { AlphaCode2 = "PE", AlphaCode3 = "PER", CountryName = "Peru", Nationality = "Peruvian" },
                    new Country() { AlphaCode2 = "PH", AlphaCode3 = "PHL", CountryName = "Philippines", Nationality = "Philippine" },
                    new Country() { AlphaCode2 = "PN", AlphaCode3 = "PCN", CountryName = "Pitcairn", Nationality = "Pitcairn Island" },
                    new Country() { AlphaCode2 = "PL", AlphaCode3 = "POL", CountryName = "Poland", Nationality = "Polish" },
                    new Country() { AlphaCode2 = "PT", AlphaCode3 = "PRT", CountryName = "Portugal", Nationality = "Portuguese" },
                    new Country() { AlphaCode2 = "PR", AlphaCode3 = "PRI", CountryName = "Puerto Rico", Nationality = "Puerto Rican" },
                    new Country() { AlphaCode2 = "QA", AlphaCode3 = "QAT", CountryName = "Qatar", Nationality = "Qatari" },
                    new Country() { AlphaCode2 = "RE", AlphaCode3 = "REU", CountryName = "Réunion", Nationality = "Réunionese" },
                    new Country() { AlphaCode2 = "RO", AlphaCode3 = "ROU", CountryName = "Romania", Nationality = "Romanian" },
                    new Country() { AlphaCode2 = "RU", AlphaCode3 = "RUS", CountryName = "Russia", Nationality = "Russian" },
                    new Country() { AlphaCode2 = "RW", AlphaCode3 = "RWA", CountryName = "Rwanda", Nationality = "Rwandan" },
                    new Country() { AlphaCode2 = "BL", AlphaCode3 = "BLM", CountryName = "Saint Barthélemy", Nationality = "Barthélemois" },
                    new Country() { AlphaCode2 = "SH", AlphaCode3 = "SHN", CountryName = "Saint Helena", Nationality = "Saint Helenian" },
                    new Country() { AlphaCode2 = "KN", AlphaCode3 = "KNA", CountryName = "Saint Kitts and Nevis", Nationality = "Kittitian" },
                    new Country() { AlphaCode2 = "LC", AlphaCode3 = "LCA", CountryName = "Saint Lucia", Nationality = "Saint Lucian" },
                    new Country() { AlphaCode2 = "MF", AlphaCode3 = "MAF", CountryName = "Saint Martin", Nationality = "Saint-Martinoise" },
                    new Country() { AlphaCode2 = "PM", AlphaCode3 = "SPM", CountryName = "Saint Pierre and Miquelon", Nationality = "Saint-Pierrais or Miquelonnais" },
                    new Country() { AlphaCode2 = "VC", AlphaCode3 = "VCT", CountryName = "Saint Vincent and the Grenadines", Nationality = "Saint Vincentian" },
                    new Country() { AlphaCode2 = "WS", AlphaCode3 = "WSM", CountryName = "Samoa", Nationality = "Samoan" },
                    new Country() { AlphaCode2 = "SM", AlphaCode3 = "SMR", CountryName = "San Marino", Nationality = "Sammarinese" },
                    new Country() { AlphaCode2 = "ST", AlphaCode3 = "STP", CountryName = "Sao Tome and Principe", Nationality = "São Toméan" },
                    new Country() { AlphaCode2 = "SA", AlphaCode3 = "SAU", CountryName = "Saudi Arabia", Nationality = "Saudi Arabian" },
                    new Country() { AlphaCode2 = "SN", AlphaCode3 = "SEN", CountryName = "Senegal", Nationality = "Senegalese" },
                    new Country() { AlphaCode2 = "RS", AlphaCode3 = "SRB", CountryName = "Serbia", Nationality = "Serbian" },
                    new Country() { AlphaCode2 = "SC", AlphaCode3 = "SYC", CountryName = "Seychelles", Nationality = "Seychellois" },
                    new Country() { AlphaCode2 = "SL", AlphaCode3 = "SLE", CountryName = "Sierra Leone", Nationality = "Sierra Leonean" },
                    new Country() { AlphaCode2 = "SG", AlphaCode3 = "SGP", CountryName = "Singapore", Nationality = "Singaporean" },
                    new Country() { AlphaCode2 = "SX", AlphaCode3 = "SXM", CountryName = "Sint Maarten", Nationality = "Sint Maarten" },
                    new Country() { AlphaCode2 = "SK", AlphaCode3 = "SVK", CountryName = "Slovakia", Nationality = "Slovak" },
                    new Country() { AlphaCode2 = "SI", AlphaCode3 = "SVN", CountryName = "Slovenia", Nationality = "Slovenian" },
                    new Country() { AlphaCode2 = "SB", AlphaCode3 = "SLB", CountryName = "Solomon Islands", Nationality = "Solomon Island" },
                    new Country() { AlphaCode2 = "SO", AlphaCode3 = "SOM", CountryName = "Somalia", Nationality = "Somalian" },
                    new Country() { AlphaCode2 = "ZA", AlphaCode3 = "ZAF", CountryName = "South Africa", Nationality = "South African" },
                    new Country() { AlphaCode2 = "GS", AlphaCode3 = "SGS", CountryName = "South Georgia and the South Sandwich Islands", Nationality = "South Georgia or South Sandwich Islands" },
                    new Country() { AlphaCode2 = "SS", AlphaCode3 = "SSD", CountryName = "South Sudan", Nationality = "South Sudanese" },
                    new Country() { AlphaCode2 = "ES", AlphaCode3 = "ESP", CountryName = "Spain", Nationality = "Spanish" },
                    new Country() { AlphaCode2 = "LK", AlphaCode3 = "LKA", CountryName = "Sri Lanka", Nationality = "Sri Lankan" },
                    new Country() { AlphaCode2 = "SD", AlphaCode3 = "SDN", CountryName = "Sudan", Nationality = "Sudanese" },
                    new Country() { AlphaCode2 = "SR", AlphaCode3 = "SUR", CountryName = "Suriname", Nationality = "Surinamese" },
                    new Country() { AlphaCode2 = "SJ", AlphaCode3 = "SJM", CountryName = "Svalbard and Jan Mayen", Nationality = "Svalbard" },
                    new Country() { AlphaCode2 = "SZ", AlphaCode3 = "SWZ", CountryName = "Swaziland", Nationality = "Swazi" },
                    new Country() { AlphaCode2 = "SE", AlphaCode3 = "SWE", CountryName = "Sweden", Nationality = "Swedish" },
                    new Country() { AlphaCode2 = "CH", AlphaCode3 = "CHE", CountryName = "Switzerland", Nationality = "Swiss" },
                    new Country() { AlphaCode2 = "SY", AlphaCode3 = "SYR", CountryName = "Syrian Arab Republic", Nationality = "Syrian" },
                    new Country() { AlphaCode2 = "TW", AlphaCode3 = "TWN", CountryName = "Taiwan", Nationality = "Chinese, Taiwanese" },
                    new Country() { AlphaCode2 = "TJ", AlphaCode3 = "TJK", CountryName = "Tajikistan", Nationality = "Tajikistani" },
                    new Country() { AlphaCode2 = "TZ", AlphaCode3 = "TZA", CountryName = "Tanzania", Nationality = "Tanzanian" },
                    new Country() { AlphaCode2 = "TH", AlphaCode3 = "THA", CountryName = "Thailand", Nationality = "Thai" },
                    new Country() { AlphaCode2 = "TL", AlphaCode3 = "TLS", CountryName = "Timor-Leste", Nationality = "Timorese" },
                    new Country() { AlphaCode2 = "TG", AlphaCode3 = "TGO", CountryName = "Togo", Nationality = "Togolese" },
                    new Country() { AlphaCode2 = "TK", AlphaCode3 = "TKL", CountryName = "Tokelau", Nationality = "Tokelauan" },
                    new Country() { AlphaCode2 = "TO", AlphaCode3 = "TON", CountryName = "Tonga", Nationality = "Tongan" },
                    new Country() { AlphaCode2 = "TT", AlphaCode3 = "TTO", CountryName = "Trinidad and Tobago", Nationality = "Trinidadian or Tobagonian" },
                    new Country() { AlphaCode2 = "TN", AlphaCode3 = "TUN", CountryName = "Tunisia", Nationality = "Tunisian" },
                    new Country() { AlphaCode2 = "TR", AlphaCode3 = "TUR", CountryName = "Turkey", Nationality = "Turkish" },
                    new Country() { AlphaCode2 = "TM", AlphaCode3 = "TKM", CountryName = "Turkmenistan", Nationality = "Turkmen" },
                    new Country() { AlphaCode2 = "TC", AlphaCode3 = "TCA", CountryName = "Turks and Caicos Islands", Nationality = "Turks and Caicos Island" },
                    new Country() { AlphaCode2 = "TV", AlphaCode3 = "TUV", CountryName = "Tuvalu", Nationality = "Tuvaluan" },
                    new Country() { AlphaCode2 = "UG", AlphaCode3 = "UGA", CountryName = "Uganda", Nationality = "Ugandan" },
                    new Country() { AlphaCode2 = "UA", AlphaCode3 = "UKR", CountryName = "Ukraine", Nationality = "Ukrainian" },
                    new Country() { AlphaCode2 = "AE", AlphaCode3 = "ARE", CountryName = "United Arab Emirates", Nationality = "Emirati" },
                    new Country() { AlphaCode2 = "GB", AlphaCode3 = "GBR", CountryName = "United Kingdom of Great Britain and Northern Ireland", Nationality = "British" },
                    new Country() { AlphaCode2 = "UM", AlphaCode3 = "UMI", CountryName = "United States Minor Outlying Islands", Nationality = "American" },
                    new Country() { AlphaCode2 = "US", AlphaCode3 = "USA", CountryName = "United States of America", Nationality = "American" },
                    new Country() { AlphaCode2 = "UY", AlphaCode3 = "URY", CountryName = "Uruguay", Nationality = "Uruguayan" },
                    new Country() { AlphaCode2 = "UZ", AlphaCode3 = "UZB", CountryName = "Uzbekistan", Nationality = "Uzbek" },
                    new Country() { AlphaCode2 = "VU", AlphaCode3 = "VUT", CountryName = "Vanuatu", Nationality = "Vanuatuan" },
                    new Country() { AlphaCode2 = "VE", AlphaCode3 = "VEN", CountryName = "Venezuela", Nationality = "Venezuelan" },
                    new Country() { AlphaCode2 = "VN", AlphaCode3 = "VNM", CountryName = "Vietnam", Nationality = "Vietnamese" },
                    new Country() { AlphaCode2 = "VG", AlphaCode3 = "VGB", CountryName = "Virgin Islands (British)", Nationality = "British Virgin Island" },
                    new Country() { AlphaCode2 = "VI", AlphaCode3 = "VIR", CountryName = "Virgin Islands (U.S.)", Nationality = "U.S. Virgin Island" },
                    new Country() { AlphaCode2 = "WF", AlphaCode3 = "WLF", CountryName = "Wallis and Futuna", Nationality = "Wallis and Futuna" },
                    new Country() { AlphaCode2 = "EH", AlphaCode3 = "ESH", CountryName = "Western Sahara", Nationality = "Sahrawi" },
                    new Country() { AlphaCode2 = "YE", AlphaCode3 = "YEM", CountryName = "Yemen", Nationality = "Yemeni" },
                    new Country() { AlphaCode2 = "ZM", AlphaCode3 = "ZMB", CountryName = "Zambia", Nationality = "Zambian" },
                    new Country() { AlphaCode2 = "ZW", AlphaCode3 = "ZWE", CountryName = "Zimbabwe", Nationality = "Zimbabwean" }
            ];
            _databaseContext.Countries.AddRange(countries);
            _databaseContext.SaveChanges();

        }

        //Company 
        if (!await _databaseContext.Company.AnyAsync())
        {
            Company company = new()
            {
                SettingId = 1,
                CompanyName = "Sales Company",
                CompanyEmailAddress = "company@mail.com",
                CompanyPhoneNumber1 = "0000000000",
                CompanyPhoneNumber2 = "0000000000",

            };
            _databaseContext.Company.Add(company);
            _databaseContext.SaveChanges();
        }

        //Employee Types
        if (!await _databaseContext.EmployeeTypes.AnyAsync())
        {
            List<EmployeeType> empTypes =
                [
                    new EmployeeType{ Name="Permanent", Description="Best for Permanent Staff"},
                        new EmployeeType{ Name="Contract", Description="Best for Contract Staff"},
                        new EmployeeType{ Name="Industrial Attachment", Description="Best for staff on Industrial Attachment"},
                        new EmployeeType{ Name="National Service", Description="Best for National Service Personnel"},
                        new EmployeeType{ Name="Temporal", Description="Best for Temporal software Users like external auditors etc"}
                ];

            _databaseContext.EmployeeTypes.AddRange(empTypes);
            _databaseContext.SaveChanges();
        }

        //Title
        if (!await _databaseContext.Titles.AnyAsync())
        {
            List<Title> titles =
                [
                    new Title{ Name="Mr"},
                        new Title{ Name="Mrs"},
                        new Title{ Name="Miss"},
                        new Title{ Name="Rev"},
                        new Title{ Name="Dr"}
                ];
            _databaseContext.Titles.AddRange(titles);
            _databaseContext.SaveChanges();
        }

        //Modules
        if (!await _databaseContext.Modules.AnyAsync())
        {
            var crud = "Users can Create, Read, Edit or Update, Delete, Export, Configure, Approve, Appoint, and View Report ";
            List<Module> systemModules = [];
            foreach (var module in SystemModules.Modules)
            {
                Module systemModule = new()
                {
                    ModuleName = module.Item2,
                    ModuleDisplay = module.Item3,
                    ModuleDescription = $"{crud} on {module.Item3}"
                };
                systemModules.Add(systemModule);
            }

            if (systemModules.Count > 0)
            {
                _databaseContext.Modules.AddRange(systemModules);
                _databaseContext.SaveChanges();
            }
        }

        //Add Role Modules
        if (!await _databaseContext.RoleModules.AnyAsync())
        {
            ApplicationRole superAdminRole = _databaseContext.Roles.AsNoTracking().FirstOrDefault(r => r.Name == ApplicationRoles.SuperAdministrator);
            ApplicationRole adminRole = _databaseContext.Roles.AsNoTracking().FirstOrDefault(r => r.Name == ApplicationRoles.Administrator);
            ApplicationRole staffRole = _databaseContext.Roles.AsNoTracking().FirstOrDefault(r => r.Name == ApplicationRoles.Staff);
            ApplicationRole userRole = _databaseContext.Roles.AsNoTracking().FirstOrDefault(r => r.Name == ApplicationRoles.ViewOnly);

            List<Module> systemModules = await _databaseContext.Modules.AsNoTracking().ToListAsync();
            List<RoleModule> roleModules = [];
            if (systemModules.Count > 0)
            {
                foreach (Module module in systemModules)
                {
                    roleModules.Add(new RoleModule() { RoleId = superAdminRole.Id, ModuleId = module.Id });
                }

                string[] adminModulesNames = [
                    ConstantModules.Profile_Module,
                        ConstantModules.User_Module,
                        ConstantModules.Employee_Module,
                        ConstantModules.System_Settings,
                        ConstantModules.Items_Module,
                        ConstantModules.Items_Requisition_Module,
                        ConstantModules.Sales_Module,
                        ConstantModules.Orders_Module,
                        ConstantModules.Stock_Module,
                        ConstantModules.Purchase_Module,
                        ConstantModules.Category_Module,
                        ConstantModules.Facility_Module,
                        ConstantModules.Inventory_Module,
                        ConstantModules.Facility_Module,
                        ConstantModules.Inspections_Module,
                        ConstantModules.Faults_Module,
                        ConstantModules.Reporting_Module,
                    ];

                string[] staffModulesNames = [
                   ConstantModules.Profile_Module,
                        ConstantModules.Items_Module,
                        ConstantModules.Sales_Module,
                        ConstantModules.Orders_Module,
                        ConstantModules.Stock_Module,
                        ConstantModules.Inventory_Module,
                        ConstantModules.Reporting_Module
               ];

                //Admin
                List<Module> adminModulesList = systemModules.Where(m => adminModulesNames.Contains(m.ModuleName)).ToList();
                foreach (Module module in adminModulesList)
                {
                    roleModules.Add(new RoleModule() { RoleId = adminRole.Id, ModuleId = module.Id });
                }
                //StaffRole
                List<Module> staffModulesList = systemModules.Where(m => staffModulesNames.Contains(m.ModuleName)).ToList();
                foreach (Module module in staffModulesList)
                {
                    roleModules.Add(new RoleModule() { RoleId = staffRole.Id, ModuleId = module.Id });
                }
            }
            else
            {
                Module userManagementModule = _databaseContext.Modules.FirstOrDefault(x => x.ModuleName == ConstantModules.User_Module);
                Module profileManagementModule = _databaseContext.Modules.FirstOrDefault(x => x.ModuleName == ConstantModules.Profile_Module);
                Module settingsModule = _databaseContext.Modules.FirstOrDefault(x => x.ModuleName == ConstantModules.System_Settings);
                //SA
                roleModules.Add(new RoleModule() { RoleId = superAdminRole.Id, ModuleId = userManagementModule.Id });
                roleModules.Add(new RoleModule() { RoleId = superAdminRole.Id, ModuleId = profileManagementModule.Id });
                roleModules.Add(new RoleModule() { RoleId = superAdminRole.Id, ModuleId = settingsModule.Id });
                //Staff
            }
            //Add Modules to role
            _databaseContext.RoleModules.AddRange(roleModules);
            _databaseContext.SaveChanges();
        }

        //Seed Default User
        if (!await _databaseContext.Users.AnyAsync())
        {
            ApplicationUser systemAdministrator = new()
            {
                Title = "Mr",
                FirstName = "System",
                LastName = "Administrator",
                OtherName = "",
                UserName = superAdminEmail,
                NormalizedUserName = superAdminEmail,
                Email = superAdminEmail,
                LockoutEnabled = false,
                PhoneNumber = "0500002517",
                PhoneNumberConfirmed = true,
                EmailConfirmed = true,
                IsActive = true,
                IsResetMode = false,
                DateCreated = DateTime.UtcNow
            };
            //Check if User Already Exists 
            if (await _databaseContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email == systemAdministrator.Email) == null)
            {
                //generate random password
                //using var rng = RandomNumberGenerator.Create();
                //var bytes = new byte[12];
                //rng.GetBytes(bytes);
                //var chars = bytes.Select(b => validChars[b % validChars.Length]);
                //var password = new string(chars.ToArray());
                var password = "r00t@@admin123!!";
                //send password to default user
                #region Email Password

                List<EmailAddress> emailRecipients = [new EmailAddress() { Name = systemAdministrator.Email, Address = systemAdministrator.Email }];
                EmailMessage mailMessage = new()
                {
                    Subject = "Account Details - Password",
                    Body = $"An account has been created with your email. Use this temporal password to login.<br> <b>{password}</b> <br> Kindly reset this password after login.",
                    EmailTemplateFilePath = _webHostEnvironment.WebRootPath + FileStorePath.EmailTemplateFile,
                    EmailLink = "",
                    ToAddresses = emailRecipients
                };

                //Email to administrator
                await _mailService.SendEmailAsync(mailMessage);

                #endregion

                //Create User
                await _userManager.CreateAsync(systemAdministrator, password);
                _databaseContext.SaveChanges();

            }

            ApplicationUser adminStaff = new()
            {
                Title = "Mr",
                FirstName = "Administrator",
                LastName = "Staff",
                OtherName = "",
                UserName = adminEmail,
                NormalizedUserName = adminEmail,
                Email = adminEmail,
                LockoutEnabled = false,
                PhoneNumber = "0600000000",
                PhoneNumberConfirmed = true,
                EmailConfirmed = true,
                IsActive = true,
                IsResetMode = false,
                DateCreated = DateTime.UtcNow
            };

            ApplicationUser oneStaff = new()
            {
                Title = "Mr",
                FirstName = "Staff",
                LastName = "Staff",
                OtherName = "",
                UserName = staffEmail,
                NormalizedUserName = staffEmail,
                Email = staffEmail,
                LockoutEnabled = false,
                PhoneNumber = "0240000000",
                PhoneNumberConfirmed = true,
                EmailConfirmed = true,
                IsActive = true,
                IsResetMode = false,
                DateCreated = DateTime.UtcNow
            };

            List<ApplicationUser> Superlist =
            [
                adminStaff,
                    oneStaff,
                ];

            foreach (var user in Superlist)
            {
                if (await _databaseContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email == user.Email) == null)
                {
                    //generate random password
                    using var rng = RandomNumberGenerator.Create();
                    var bytes = new byte[12];
                    rng.GetBytes(bytes);
                    var chars = bytes.Select(b => validChars[b % validChars.Length]);
                    var password = new string(chars.ToArray());

                    //send password to default user
                    #region Email Password

                    List<EmailAddress> emailRecipients = [new EmailAddress() { Name = user.Email, Address = user.Email }];
                    EmailMessage mailMessage = new()
                    {
                        Subject = "Account Details - Password",
                        Body = $"An account has been created with your email. Use this temporal password to login.<br> <b>{password}</b> <br> Kindly reset this password after login.",
                        EmailTemplateFilePath = _webHostEnvironment.WebRootPath + FileStorePath.EmailTemplateFile,
                        EmailLink = "",
                        ToAddresses = emailRecipients
                    };

                    //Email to administrator
                    await _mailService.SendEmailAsync(mailMessage);

                    #endregion

                    //Create User
                    await _userManager.CreateAsync(user, password);
                    _databaseContext.SaveChanges();

                }
            }
        }

        //Add User to Role
        if (!await _databaseContext.UserRoles.AnyAsync())
        {
            ApplicationRole superAdminRole = _databaseContext.Roles.AsNoTracking().FirstOrDefault(r => r.Name == ApplicationRoles.SuperAdministrator);
            ApplicationRole adminRole = _databaseContext.Roles.AsNoTracking().FirstOrDefault(r => r.Name == ApplicationRoles.Administrator);
            ApplicationRole staffRole = _databaseContext.Roles.AsNoTracking().FirstOrDefault(r => r.Name == ApplicationRoles.Staff);

            List<ApplicationUser> users = await _databaseContext.Users.AsNoTracking().ToListAsync();

            List<IdentityUserRole<int>> userRoleList = [];

            foreach (ApplicationUser user in users)
            {
                if (user.Email == superAdminEmail)
                {
                    userRoleList.Add(new IdentityUserRole<int> { RoleId = superAdminRole.Id, UserId = user.Id });
                }
                else if (user.Email == adminEmail)
                {
                    userRoleList.Add(new IdentityUserRole<int> { RoleId = adminRole.Id, UserId = user.Id });
                }
                else if (user.Email == staffEmail)
                {
                    userRoleList.Add(new IdentityUserRole<int> { RoleId = staffRole.Id, UserId = user.Id });
                }
                else { }
            }

            if (userRoleList.Count > 0)
            {
                _databaseContext.UserRoles.AddRange(userRoleList);
                _databaseContext.SaveChanges();
            }
        }

        //Add Employee
        //if (!await _databaseContext.Employees.AnyAsync())
        //{
        //    List<Employee> employees = [];
        //    List<ApplicationUser> users = await _databaseContext.Users.AsNoTracking().ToListAsync();
        //    if (users.Count > 0)
        //    {
        //        int i = 0;
        //        foreach (ApplicationUser user in users)
        //        {
        //            i++;
        //            Employee oneEmployee = new()
        //            {
        //                EmployeeTypeId = 1,
        //                StaffNumber = i.ToString(),
        //                StartDate = DateTime.UtcNow,
        //                UserId = user.Id
        //            };
        //            employees.Add(oneEmployee);
        //        }
        //    }

        //    if (employees.Count > 0)
        //    {
        //        _databaseContext.Employees.AddRange(employees);
        //        _databaseContext.SaveChanges();
        //    }

        //}

        //Add Role Permissions for super admin
        if (!await _databaseContext.ModulePermissions.AnyAsync())
        {
            ApplicationRole superAdminRole = _databaseContext.Roles.FirstOrDefault(r => r.Name == ApplicationRoles.SuperAdministrator);
            ApplicationUser superAdminUser = await _userManager.FindByEmailAsync(superAdminEmail);

            var modules = await _databaseContext.Modules.AsNoTracking().ToListAsync();

            List<ModulePermission> modulePermissions = [];

            foreach (Module module in modules)
            {
                ModulePermission permission = new()
                {
                    RoleId = superAdminRole.Id,
                    UserId = superAdminUser.Id,
                    ModuleId = module.Id,
                    Create = true,
                    Read = true,
                    Update = true,
                    Delete = true,
                    Export = true,
                    Configure = true,
                    Appoint = true,
                    Approve = true,
                    Report = true
                };
                modulePermissions.Add(permission);
            }

            if (superAdminRole != null && superAdminUser != null && modulePermissions.Count > 0)
            {
                _databaseContext.ModulePermissions.AddRange(modulePermissions);
                _databaseContext.SaveChanges();
            }
        }

        //Cron Jobs
        if (!await _databaseContext.CronJobs.AnyAsync())
        {
            CronJob defaultJob = new()
            {
                Name = Constants.DefaultCronJob,
                Time = "* * * * *",
                Intervals = "3,2,1"
            };
            _databaseContext.CronJobs.Add(defaultJob);
            _databaseContext.SaveChanges();
        }


        //if (!context.Users.Any())
        //{
        //        using var transaction = context.Database.BeginTransaction();
        //        try
        //        {
        //            transaction.Commit();
        //        }
        //        catch (Exception ex)
        //        {
        //            _ = ex.Message;
        //            transaction.Rollback();
        //        }
        //}



    }

}

