using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TK.Twitter.Crawl.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTwitterUserTableAddScreenNameLowerIndexLower : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.IsNpgsql())
            {
                migrationBuilder.Sql(@"
DROP INDEX IF EXISTS  ""IX_twitter_user_ScreenName_lower"";
CREATE INDEX ""IX_twitter_user_ScreenName_lower"" ON ""public"".""twitter_user"" USING btree (
  lower(""ScreenName"") COLLATE ""pg_catalog"".""default"" ""pg_catalog"".""text_ops"" ASC NULLS LAST
);");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.IsNpgsql())
            {
                migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_twitter_user_ScreenName_lower"";");
            }
        }
    }
}
