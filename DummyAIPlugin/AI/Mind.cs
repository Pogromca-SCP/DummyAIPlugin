using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DummyAIPlugin.AI;

/// <summary>
/// Basic GOAP AI mind implementation.
/// </summary>
public class Mind
{
    /// <summary>
    /// Contains beliefs which AI uses to analyze world/game state.
    /// </summary>
    public Dictionary<string, Belief> Beliefs { get; } = [];

    /// <summary>
    /// Contains actions which AI can perform.
    /// </summary>
    public HashSet<Action> Actions { get; } = [];

    /// <summary>
    /// Contains goals which AI wants to achieve.
    /// </summary>
    public HashSet<Goal> Goals { get; } = [];

    /// <summary>
    /// Contains currently executed action.
    /// </summary>
    private Action? _currentAction = null;

    /// <summary>
    /// Contains the goal which AI is trying to achieve.
    /// </summary>
    private Goal? _currentGoal = null;

    /// <summary>
    /// Contains last goal the AI tried to achieve.
    /// </summary>
    private Goal? _lastGoal = null;

    /// <summary>
    /// Contains currently executed action plan.
    /// </summary>
    private ActionPlan? _actionPlan = null;

    /// <summary>
    /// Performs cleanup after mind usage.
    /// </summary>
    public virtual void Terminate() {}

    /// <summary>
    /// Performs AI update.
    /// </summary>
    public void Update()
    {
        if (_currentAction is null)
        {
            CalculatePlan();

            if (_actionPlan is not null && _actionPlan.Actions.Count > 0)
            {
                _currentGoal = _actionPlan.AgentGoal;
                _currentAction = _actionPlan.Actions.Pop();

                if (_currentAction.Preconditions.All(b => b.Evaluate()))
                {
                    _currentAction.Start();
                }
                else
                {
                    _currentAction = null;
                    _currentGoal = null;
                }
            }
        }

        if (_actionPlan is not null && _currentAction is not null)
        {
            _currentAction.Update();

            if (_currentAction.Complete)
            {
                _currentAction.Stop();
                _currentAction = null;

                if (_actionPlan.Actions.Count < 1)
                {
                    _lastGoal = _currentGoal;
                    _currentGoal = null;
                }
            }
        }
    }

    /// <summary>
    /// Recalculates action plan.
    /// </summary>
    public void CalculatePlan()
    {
        var priorityLevel = _currentGoal?.Priority ?? 0.0f;
        var goalsToCheck = Goals;

        if (_currentGoal is not null)
        {
            goalsToCheck = [..Goals.Where(g => g.Priority > priorityLevel)];
        }

        var potentialPlan = Plan(goalsToCheck, _lastGoal);

        if (potentialPlan is not null)
        {
            _actionPlan = potentialPlan;
        }
    }

    /// <summary>
    /// Builds an action plan visualization text.
    /// </summary>
    /// <param name="sb">String builder to use.</param>
    public void DisplayActionPlan(StringBuilder sb)
    {
        var actionPlan = _actionPlan;

        if (actionPlan is null)
        {
            sb.Append("<color=red>No action plan available</color>\n");
            return;
        }

        sb.Append("Goal: ").Append(actionPlan.AgentGoal.Name).Append('(').Append(actionPlan.TotalCost).Append(")\n");

        foreach (var action in actionPlan.Actions.Reverse())
        {
            sb.Append("  -> ");
            DisplayAction(sb, action);
            sb.Append('\n');
        }

        if (_currentAction is not null)
        {
            sb.Append("  <color=yellow>-> ");
            DisplayAction(sb, _currentAction);
            sb.Append("</color>\n");
        }
    }

    /// <summary>
    /// Builds an action visualization text.
    /// </summary>
    /// <param name="sb">String builder to use.</param>
    /// <param name="action">Action to display.</param>
    private void DisplayAction(StringBuilder sb, Action action)
    {
        sb.Append(action.Name).Append('(').Append(' ');

        foreach (var cond in action.Preconditions)
        {
            sb.Append(cond.Name).Append(' ');
        }

        sb.Append("-> ");

        foreach (var effect in action.Effects)
        {
            sb.Append(effect.Name).Append(' ');
        }

        sb.Append(')');
    }

    /// <summary>
    /// Attempts to create an action plan.
    /// </summary>
    /// <param name="goals">Available goals.</param>
    /// <param name="mostRecentGoal">Most recent goal.</param>
    /// <returns>A created action plan or <see langword="null" /> if plan creation failed.</returns>
    private ActionPlan? Plan(HashSet<Goal> goals, Goal? mostRecentGoal = null) {
        var orderedGoals = goals
            .Where(g => g.DesiredEffects.Any(b => !b.Evaluate()))
            .OrderByDescending(g => g == mostRecentGoal ? g.Priority - 0.01f : g.Priority);
        
        foreach (var goal in orderedGoals)
        {
            var goalNode = new Node(null, null, goal.DesiredEffects, 0);
            
            if (FindPath(goalNode, Actions))
            {
                if (goalNode.IsLeafDead)
                {
                    continue;
                }
                
                var actionStack = new Stack<Action>();

                while (goalNode.Leaves.Count > 0)
                {
                    var cheapestLeaf = goalNode.Leaves.OrderBy(leaf => leaf.Cost).First();
                    goalNode = cheapestLeaf;
                    actionStack.Push(cheapestLeaf.Action!);
                }
                
                return new(goal, actionStack, goalNode.Cost);
            }
        }
        
        return null;
    }

    /// <summary>
    /// Attempts to find a node path for actions.
    /// </summary>
    /// <param name="parent">Parent node.</param>
    /// <param name="actions">Available actions.</param>
    /// <returns><see langword="true" /> when managed to find a path, <see langword="false" /> otherwise.</returns>
    private bool FindPath(Node parent, HashSet<Action> actions) {
        var orderedActions = actions.OrderBy(a => a.Cost);
        
        foreach (var action in orderedActions)
        {
            var requiredEffects = parent.RequiredEffects;
            requiredEffects.RemoveWhere(b => b.Evaluate());
            
            if (requiredEffects.Count < 1)
            {
                return true;
            }

            if (action.Effects.Any(requiredEffects.Contains))
            {
                var newRequiredEffects = new HashSet<Belief>(requiredEffects);
                newRequiredEffects.ExceptWith(action.Effects);
                newRequiredEffects.UnionWith(action.Preconditions);
                var newAvailableActions = new HashSet<Action>(actions);
                newAvailableActions.Remove(action);
                var newNode = new Node(parent, action, newRequiredEffects, parent.Cost + action.Cost);
                
                if (FindPath(newNode, newAvailableActions))
                {
                    parent.Leaves.Add(newNode);
                    newRequiredEffects.ExceptWith(newNode.Action!.Preconditions);
                }
                
                if (newRequiredEffects.Count < 1)
                {
                    return true;
                }
            }
        }
        
        return parent.Leaves.Count > 0;
    }
}
