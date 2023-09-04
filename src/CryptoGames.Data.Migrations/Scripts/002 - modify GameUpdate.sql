ALTER PROCEDURE [dbo].[GameUpdate]
	@Id bigint NULL,
	@Type nvarchar(50),
	@UserName nvarchar(50),
	@Network nvarchar(50),
	@Status nvarchar(50),
	@Data nvarchar(max),
	@Size nvarchar(10)
AS
BEGIN
		SET NOCOUNT ON;

		UPDATE dbo.Games SET
                Status = @Status,
                Data = @Data
            WHERE Id = @Id AND
                --[Type] = @Type AND
                [Status] = 'Alive' --AND
                --UserName = @UserName

		IF @@ROWCOUNT = 0
        BEGIN
			--DECLARE @Id bigint
			--SET @Id = NEXT VALUE FOR [dbo].[GameSequence]
            INSERT INTO dbo.Games(Id,Type, UserName, Network, Status, Data, Size)
            VALUES (@Id, @Type, @UserName, @Network, @Status, @Data, @Size)
        END

		SELECT TOP(1) * FROM dbo.Games where Id = SCOPE_IDENTITY()
END
