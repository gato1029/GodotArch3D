using Godot;
using RVO;
using System;
using System.Collections.Generic;

namespace GodotFlecs.sources.Flecs
{
    public class RvoManager
    {
        private static RvoManager _instance;
        public static RvoManager Instance => _instance ??= new RvoManager();

        private float timeStep = 0.25f;

        private RvoManager()
        {

            Simulator.Instance.setTimeStep(timeStep);
            Simulator.Instance.setAgentDefaults(
              neighborDist: 0.5f,      // detecta otros dentro de 5 unidades
                maxNeighbors: 5,      // considera hasta 10
                timeHorizon: 3f,       // predice 3s hacia adelante
                timeHorizonObst: 2f,   // similar para obstáculos
                radius: 2f,          // tamaño de unidad
                maxSpeed: 3f,          // velocidad máxima realista
                velocity: new Vector2(0, 0)
            );
        }

        public int RegisterAgent(Vector2 position,float radio, float maxSpeed)
        {           
            int id = Simulator.Instance.addAgent(position);
            Simulator.Instance.setAgentRadius(id, radio);
            // 3️⃣ Configurar velocidad máxima específica para este agente
            Simulator.Instance.setAgentMaxSpeed(id, maxSpeed);
            return id;
        }

        public void RemoveAgent(int id)
        {
            Simulator.Instance.setAgentPosition(id, new Vector2(99999, 99999));

        }

        public void SetAgentState(int id, Vector2 position, Vector2 prefVelocity)
        {
            Simulator.Instance.setAgentPosition(id, position);
            Simulator.Instance.setAgentPrefVelocity(id, prefVelocity);
            
        }

        public Vector2 GetAgentVelocity(int id)
        {
            return Simulator.Instance.getAgentVelocity(id);
        }

        public void Step()
        {
            Simulator.Instance.doStep();
        }
    }
}
