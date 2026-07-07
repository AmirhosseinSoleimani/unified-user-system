namespace UnifiedUserSystem.src.Application.Security;

public static class OperationPolicyNames
{
    public const string Prefix = "OP:";

    public const string UsersRead = "OP:users.read";
    public const string UsersUpdate = "OP:users.update";
    public const string UsersDeactivate = "OP:users.deactivate";
    public const string UsersRolesAssign = "OP:users.roles.assign";
    public const string UsersRolesRemove = "OP:users.roles.remove";
    public const string UsersRolesReplace = "OP:users.roles.replace";

    public const string RolesRead = "OP:role.read";
    public const string RolesCreate = "OP:role.create";
    public const string RolesUpdate = "OP:role.update";
    public const string RolesRename = "OP:role.rename";
    public const string RolesDelete = "OP:role.delete";
    public const string RolesActivate = "OP:role.activate";
    public const string RolesDeactivate = "OP:role.deactivate";
    public const string RolesRemove = "OP:role.remove";

    public const string RolesOperationsRead = "OP:roles.operations.read";
    public const string RolesOperationsAssign = "OP:roles.operations.assign";
    public const string RolesOperationsRemove = "OP:roles.operations.remove";
    public const string RolesOperationsReplace = "OP:roles.operations.replace";

    public const string OperationsRead = "OP:operation.read";
    public const string OperationsCreate = "OP:operation.create";
    public const string OperationsUpdate = "OP:operation.update";
    public const string OperationsRenameTitle = "OP:operation.renameTitle";
    public const string OperationsChangeKey = "OP:operation.changeKey";
    public const string OperationsDelete = "OP:operation.delete";
    public const string OperationsActivate = "OP:operation.activate";
    public const string OperationsDeactivate = "OP:operation.deactivate";

    public const string PermissionsGrant = "OP:permission.grant";
    public const string PermissionsRevoke = "OP:permission.revoke";

    public static bool TryGetOperationKey(string? policyName, out string operationKey)
    {
        operationKey = string.Empty;

        if (string.IsNullOrWhiteSpace(policyName))
            return false;

        if (!policyName.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
            return false;

        operationKey = policyName[Prefix.Length..].Trim().ToLowerInvariant();

        return !string.IsNullOrWhiteSpace(operationKey);
    }

    public static string NormalizeOperationKey(string? operationKey)
    {
        operationKey = (operationKey ?? string.Empty).Trim();

        if (operationKey.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
            operationKey = operationKey[Prefix.Length..];

        return operationKey.Trim().ToLowerInvariant();
    }
}