using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class SplitFullNameAndAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Add new columns (nullable first to avoid immediate constraint issues)
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MiddleName",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PermanentFullAddress",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);

            // DOB split (keep original DOB column intact for now; you can migrate into these if you want)
            migrationBuilder.AddColumn<string>(
                name: "DOB_AD",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DOB_BS",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);

            // Contact mirrors (optional)
            migrationBuilder.AddColumn<string>(
                name: "EmailAddress",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNo",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MobileNo",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);

            // Father/Spouse
            migrationBuilder.AddColumn<string>(
                name: "FatherOrSpouseFirstName",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FatherOrSpouseMiddleName",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FatherOrSpouseLastName",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);

            // Citizenship/Passport
            migrationBuilder.AddColumn<string>(
                name: "CitizenshipOrPassportNo",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CitizenshipOrPassportIssuedFrom",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CitizenshipImageUrl",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PassportImageUrl",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);

            // Delivery Address
            migrationBuilder.AddColumn<bool>(
                name: "IsDeliverySameAsPermanent",
                table: "AspNetUsers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryFullAddress",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryZipCode",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryCity",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryCountry",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);

            // Nominee
            migrationBuilder.AddColumn<string>(
                name: "NomineeName",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomineeRelation",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);

            // Bank
            migrationBuilder.AddColumn<string>(
                name: "NameOnAccount",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankBranchName",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);

            // VAT/PAN (new, do NOT rename old columns into these)
            migrationBuilder.AddColumn<string>(
                name: "VatPanName",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VatPanRegistrationNumber",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VatPanIssuedFrom",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);

            // Profile picture
            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureUrl",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);

            // Permanent address breakdown (optional; you already had PermanentFullAddress)
            migrationBuilder.AddColumn<string>(
                name: "PermanentZipCode",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PermanentCity",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PermanentCountry",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true);

            // 2) Copy data from old columns BEFORE dropping them
            // Split FullName → First/Middle/Last
            migrationBuilder.Sql(@"
                UPDATE `AspNetUsers`
                SET
                  `FirstName` = TRIM(SUBSTRING_INDEX(`FullName`, ' ', 1)),
                  `LastName`  = TRIM(SUBSTRING_INDEX(`FullName`, ' ', -1)),
                  `MiddleName` = NULLIF(TRIM(
                      SUBSTRING(
                          `FullName`,
                          LENGTH(SUBSTRING_INDEX(`FullName`, ' ', 1)) + 2,
                          LENGTH(`FullName`)
                            - LENGTH(SUBSTRING_INDEX(`FullName`, ' ', 1))
                            - LENGTH(SUBSTRING_INDEX(`FullName`, ' ', -1)) - 1
                      )
                  ), '')
                WHERE `FullName` IS NOT NULL AND `FullName` <> '';
            ");

            // Address → PermanentFullAddress
            migrationBuilder.Sql(@"
                UPDATE `AspNetUsers`
                SET `PermanentFullAddress` = `Address`
                WHERE (`PermanentFullAddress` IS NULL OR `PermanentFullAddress` = '')
                  AND `Address` IS NOT NULL;
            ");

            // Optional convenience: copy Identity Email/Phone into your new mirrors
            migrationBuilder.Sql(@"
                UPDATE `AspNetUsers`
                SET `EmailAddress` = `Email`
                WHERE `Email` IS NOT NULL AND (`EmailAddress` IS NULL OR `EmailAddress` = '');
            ");

            migrationBuilder.Sql(@"
                UPDATE `AspNetUsers`
                SET `PhoneNo` = `PhoneNumber`, `MobileNo` = `PhoneNumber`
                WHERE `PhoneNumber` IS NOT NULL
                  AND (`PhoneNo` IS NULL OR `PhoneNo` = '')
                  AND (`MobileNo` IS NULL OR `MobileNo` = '');
            ");

            // 3) Now safely drop the old columns
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "AspNetUsers");

            // NOTE:
            // We intentionally DO NOT rename/drop old `DOB`, `CitizenshipNo`, or `AccountName` here.
            // Keep them until you explicitly migrate those values into the new VAT/PAN fields or elsewhere.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1) Recreate old columns
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "longtext",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "AspNetUsers",
                type: "longtext",
                nullable: false,
                defaultValue: "");

            // 2) Rebuild FullName and Address from new columns
            migrationBuilder.Sql(@"
                UPDATE `AspNetUsers`
                SET `FullName` = TRIM(CONCAT(
                    COALESCE(`FirstName`, ''), ' ',
                    COALESCE(`MiddleName`, ''), ' ',
                    COALESCE(`LastName`, '')
                ));
            ");

            migrationBuilder.Sql(@"
                UPDATE `AspNetUsers`
                SET `Address` = COALESCE(`PermanentFullAddress`, '')
            ");

            // 3) Drop newly added columns
            migrationBuilder.DropColumn(name: "FirstName", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "MiddleName", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "LastName", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "PermanentFullAddress", table: "AspNetUsers");

            migrationBuilder.DropColumn(name: "DOB_AD", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "DOB_BS", table: "AspNetUsers");

            migrationBuilder.DropColumn(name: "EmailAddress", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "PhoneNo", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "MobileNo", table: "AspNetUsers");

            migrationBuilder.DropColumn(name: "FatherOrSpouseFirstName", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "FatherOrSpouseMiddleName", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "FatherOrSpouseLastName", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "Gender", table: "AspNetUsers");

            migrationBuilder.DropColumn(name: "CitizenshipOrPassportNo", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "CitizenshipOrPassportIssuedFrom", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "CitizenshipImageUrl", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "PassportImageUrl", table: "AspNetUsers");

            migrationBuilder.DropColumn(name: "IsDeliverySameAsPermanent", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "DeliveryFullAddress", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "DeliveryZipCode", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "DeliveryCity", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "DeliveryCountry", table: "AspNetUsers");

            migrationBuilder.DropColumn(name: "NomineeName", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "NomineeRelation", table: "AspNetUsers");

            migrationBuilder.DropColumn(name: "NameOnAccount", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "BankBranchName", table: "AspNetUsers");

            migrationBuilder.DropColumn(name: "VatPanName", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "VatPanRegistrationNumber", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "VatPanIssuedFrom", table: "AspNetUsers");

            migrationBuilder.DropColumn(name: "ProfilePictureUrl", table: "AspNetUsers");

            migrationBuilder.DropColumn(name: "PermanentZipCode", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "PermanentCity", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "PermanentCountry", table: "AspNetUsers");

            // IMPORTANT:
            // We never touched old `DOB`, `CitizenshipNo`, or `AccountName` here,
            // so no need to restore them from VAT/PAN fields.
        }
    }
}
