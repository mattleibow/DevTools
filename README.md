# Developer Tools (dev-tools)

A repository of various GitHub Actions, Azure Functions and
other tools that make maintaining an open source (or any)
repository easier.

There are a few things so far:

* Main Azure backend on the `main` branch
* Github Actions on the `action/*` branches:
  * [`action/label-with-ai`](#ai-issue-labeler)  
  * [`action/engagement`](#engagement-score)  

## Actions

### AI Issue Labeler

Issue labeller which applies one label from a set of
labels to issues based on the title and description.

See the [`action/label-with-ai`](https://github.com/mattleibow/dev-tools/tree/action/label-with-ai) branch.

#### Example Labeler Workflow

```yml
name: Apply labels using AI
on:
  issues:
    types: [opened]
jobs:
  apply-label:
    runs-on: ubuntu-latest
    permissions:
      issues: write # write permissions to update the issue
    steps:
      - uses: mattleibow/dev-tools@action/label-with-ai
        with:
          # Labels to pick from
          labels: |                  # multiline string of exact labels
            area/action
            area/ai
            area/function
          label-pattern: '^area/.*'  # a regex pattern to match labels
          # Controls
          apply-label: true          # apply the label automatically
          add-comment: true          # add a comment to explain the label
```

### Engagement Score

Project updater which updates a specifric column in
a specific project with a calculated engagement score
to help prioritize and/or raise awareness of hot
issues.

See the [`action/engagement`](https://github.com/mattleibow/dev-tools/tree/action/engagement) branch.

TODO

#### Example Engagement Workflow

TODO
