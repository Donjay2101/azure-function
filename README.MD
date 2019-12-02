# Bitbucket

Team Lead for MyEvents : **Ahmad Nazmi Anuar (nazmi.anuar@petronas.com)**

> Default branches are **master** branch and **develop** branch.

**master** branch - Only DevOps/Admin team can commit
**develop** branch - Require developers to create pull request for team lead & reviewers to review before merging into develop branch

> Workflow for development:
1. Developers may work on the features by creating a new feature branch or custom branch. 
2. Upon completing the feature, create a pull request to merge into develop branch. 
3. Team Lead & reviewers will review the pull request and merge into develop branch. 
4. Develop branch will be deployed to development server for **feature testing**. 
5. QA and developers will test the feature in development server and do the fixes accordingly. 

> Workflow for production:
1. Prior to production deployment, team lead needs to inform DevOps/Admin team to create a release branch.
2. DevOps/Admin team will branch out a new release branch from the develop branch. 
3. The new release branch will be deployed to staging environment for **full testing**.
4. If no issues found in staging environment, the release branch will be used later for production deployment.  

> First time setup:
1. Create a feature/custom branch and commit source code to it.
2. Create a pull request to merge into the develop branch. 
3. Contact Team Lead/Reviewers to review and approve the pull request for merging into develop branch.