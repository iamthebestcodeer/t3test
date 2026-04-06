using T3Code.Core.Models;

namespace T3Code.Core.Tests;

public class GitModelsTests
{
    [Fact]
    public void GitStatus_Empty_CreatesCorrectly()
    {
        var status = GitStatus.Empty("/repo/path");

        Assert.Equal(string.Empty, status.Branch);
        Assert.Equal("/repo/path", status.Cwd);
        Assert.Empty(status.Files);
        Assert.Equal(0, status.Ahead);
        Assert.Equal(0, status.Behind);
    }

    [Fact]
    public void GitBranch_StoresCorrectly()
    {
        var branch = new GitBranch
        {
            Name = "feature/test",
            IsCurrent = true,
            IsRemote = false,
            Upstream = "origin/feature/test",
        };

        Assert.Equal("feature/test", branch.Name);
        Assert.True(branch.IsCurrent);
        Assert.False(branch.IsRemote);
        Assert.Equal("origin/feature/test", branch.Upstream);
    }

    [Fact]
    public void GitStatusFile_StoresCorrectly()
    {
        var file = new GitStatusFile
        {
            Path = "src/main.cs",
            Status = GitFileStatus.Modified,
            Staged = true,
        };

        Assert.Equal("src/main.cs", file.Path);
        Assert.Equal(GitFileStatus.Modified, file.Status);
        Assert.True(file.Staged);
    }

    [Fact]
    public void GitFileStatus_AllValues_Covered()
    {
        var values = Enum.GetValues<GitFileStatus>();
        Assert.Equal(8, values.Length);
        Assert.Contains(GitFileStatus.Modified, values);
        Assert.Contains(GitFileStatus.Added, values);
        Assert.Contains(GitFileStatus.Deleted, values);
        Assert.Contains(GitFileStatus.Renamed, values);
        Assert.Contains(GitFileStatus.Copied, values);
        Assert.Contains(GitFileStatus.Untracked, values);
        Assert.Contains(GitFileStatus.Ignored, values);
        Assert.Contains(GitFileStatus.Unmerged, values);
    }

    [Fact]
    public void GitWorktree_StoresCorrectly()
    {
        var wt = new GitWorktree
        {
            Path = "/repo/worktree-1",
            Branch = "feature-branch",
            Head = "abc1234",
        };

        Assert.Equal("/repo/worktree-1", wt.Path);
        Assert.Equal("feature-branch", wt.Branch);
        Assert.Equal("abc1234", wt.Head);
    }

    [Fact]
    public void PullRequestRef_StoresCorrectly()
    {
        var pr = new PullRequestRef
        {
            Number = "42",
            Title = "Add feature X",
            Url = "https://github.com/org/repo/pull/42",
            HeadRef = "feature-x",
            BaseRef = "main",
            State = "open",
        };

        Assert.Equal("42", pr.Number);
        Assert.Equal("Add feature X", pr.Title);
        Assert.Equal("open", pr.State);
    }
}
