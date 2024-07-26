using Godot;
using System;
using System.Collections.Generic;

public partial class player : CharacterBody2D
{
	public const float ACCELERATION = 800.0f;
	public const float FRICTION = 750.0f;
    public const float MAX_SPEED = 150.0f;

    public enum PlayerState
	{
		IDLE,
		RUN
	}

	private PlayerState current_state;
	private AnimationTree animationTree;
	private AnimationNodeStateMachinePlayback stateMachine;
	private Vector2 blend_position;
	private Vector2 _velocity;
    private Vector2 last_direction;
    private List<String> blend_pos_paths;
    private Dictionary<PlayerState, string> animTree_state_keys = new Dictionary<PlayerState, string>
	{
		{ PlayerState.IDLE, "idle" },
		{ PlayerState.RUN, "run" }
	};

	public override void _Ready()
	{
        // Retrieve AnimationTree and state machine
        animationTree = GetNode<AnimationTree>("AnimationTree");
		animationTree.Active = true;
		stateMachine = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");

        blend_pos_paths= new List<string>
        {
        "parameters/idle/idle_b2sd/blend_position",
        "parameters/run/run_bs2d/blend_position"
		};
    }

    public override void _PhysicsProcess(double delta)
    {
		Move(delta);
		Animate();
	}

	public void Move(double delta)
	{

		Vector2 input_vector = Input.GetVector("move_left", "move_right", "move_up", "move_down");


		if (input_vector == Vector2.Zero)
		{
            current_state = PlayerState.IDLE;
            ApplyFriction(FRICTION * (float)delta);
        }
		else
		{
            current_state = PlayerState.RUN;
            ApplyMovement(input_vector * ACCELERATION * (float)delta);
            blend_position = input_vector;
			last_direction = input_vector;
        }

        Velocity = _velocity;
        MoveAndSlide();
	}

	private void ApplyMovement(Vector2 amount)
	{
		_velocity += amount;
		_velocity = _velocity.LimitLength(MAX_SPEED);
	}

    private void ApplyFriction(float amount)
    {
        if (_velocity.Length() > amount)
        {
            _velocity -= _velocity.Normalized() * amount;
        }
        else
        {
            _velocity = Vector2.Zero;
		}
	}

	private void Animate()
	{
		stateMachine.Travel(animTree_state_keys[current_state]);
        animationTree.Set(
			blend_pos_paths[(int)current_state],
			current_state == PlayerState.RUN ? blend_position : last_direction
		);
    }

}

