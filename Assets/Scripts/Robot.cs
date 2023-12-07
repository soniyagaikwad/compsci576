using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class Robot : MonoBehaviour
{
    public GameObject fps_player_obj;
    public float robot_speed;
    public float rotationSpeed = 30.0f;
    public string type = "Sphere";
    public int life = 3;
    private float radius_of_search_for_player;
    private TMP_Text[] textComponents;
    private int currentValue;

    void Start()
    {
        GameObject plane = GameObject.FindGameObjectWithTag("Plane");
        Bounds bounds = plane.GetComponent<Collider>().bounds;
        radius_of_search_for_player = (bounds.size.x + bounds.size.z) / 5.0f;

        textComponents = new TMP_Text[4];
        string expression = "";
        if (type == "Sphere")
        {
            int number = Random.Range(0, 10);
            expression = number.ToString();
            currentValue = number;
        } else if (type == "Rectangle")
        {
            string original = GenerateExpression();
            int idx = -1;
            for (int i = 0; i < original.Length; i++)
            {
                if (!char.IsDigit(original[i]))
                {
                    idx = i;
                    break;
                }
            }
            int A = int.Parse(original.Substring(0, idx));
            char op = original[idx];
            int B = int.Parse(original.Substring(idx + 1));
            expression = A + "\n" + op + "\n" + B;
            currentValue = EvaluateExpression(original);
        } else if (type == "Cube")
        {
            expression = GenerateExpression();
            //Debug.Log(expression + " Cube expression");
            currentValue = EvaluateExpression(expression);
            //Debug.Log(currentValue + " Cube result");
        }
        for (int i = 0; i < textComponents.Length; i++)
        {
            textComponents[i] = transform.GetChild(i).GetComponent<TMP_Text>();
            textComponents[i].text = expression;
        }
    }
    public static string GenerateExpression()
    {
        int A, B;
        char[] operators = new char[] { '+', '-', 'x', '/' };
        char op = operators[Random.Range(0, operators.Length)];
        do
        {
            A = Random.Range(0, 10);
            B = Random.Range(0, 10);
            if (op == '/')
            {
                A = B * Random.Range(0, 10);
            }
        } while (!IsResultInRange(A, B, op));

        return $"{A}{op}{B}";
    }
    private static bool IsResultInRange(int A, int B, char op)
    {
        int result = 0;
        switch (op)
        {
            case '+':
                result = A + B;
                break;
            case '-':
                result = A - B;
                break;
            case 'x':
                result = A * B;
                break;
            case '/':
                if (B == 0)
                {
                    result = -1;
                } else
                {
                    result = A / B;
                }
                break;
        }
        return result >= 0 && result <= 9;
    }
    public static int EvaluateExpression(string expression)
    {
        int idx = -1;
        for (int i = 0; i < expression.Length; i++)
        {
            if (!char.IsDigit(expression[i]))
            {
                idx = i;
                break;
            }
        }
        int A = int.Parse(expression.Substring(0, idx));
        char op = expression[idx];
        int B = int.Parse(expression.Substring(idx + 1));
        switch (op)
        {
            case '+':
                return A + B;
            case '-':
                return A - B;
            case 'x':
                return A * B;
            case '/':
                return A / B;
        }
        return -1;
    }
    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        if (fps_player_obj != null)
        {
            Vector3 playerDirection = fps_player_obj.transform.position - transform.position;
            playerDirection.y = 0;
            if (playerDirection.magnitude < radius_of_search_for_player)
            {
                Vector3 moveDirection = playerDirection.normalized * robot_speed * Time.deltaTime;
                transform.position += moveDirection;
                transform.LookAt(new Vector3(fps_player_obj.transform.position.x, transform.position.y, fps_player_obj.transform.position.z));
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if ((collision.gameObject.name.Contains("Number One") && currentValue == 1) || 
            (collision.gameObject.name.Contains("Number Two") && currentValue == 2) ||
            (collision.gameObject.name.Contains("Number Three") && currentValue == 3) ||
            (collision.gameObject.name.Contains("Number Four") && currentValue == 4) ||
            (collision.gameObject.name.Contains("Number Five") && currentValue == 5) ||
            (collision.gameObject.name.Contains("Number Six") && currentValue == 6) ||
            (collision.gameObject.name.Contains("Number Seven") && currentValue == 7) ||
            (collision.gameObject.name.Contains("Number Eight") && currentValue == 8) ||
            (collision.gameObject.name.Contains("Number Nine") && currentValue == 9) ||
            (collision.gameObject.name.Contains("Number Zero") && currentValue == 0))
        {
            if (type == "Cube")
            {
                string expression = "";
                if (life > 1)
                {
                    life = life - 1;
                    expression = GenerateExpression();
                    currentValue = EvaluateExpression(expression);
                    for (int i = 0; i < textComponents.Length; i++)
                    {
                        textComponents[i] = transform.GetChild(i).GetComponent<TMP_Text>();
                        textComponents[i].text = expression;
                    }
            } else
                {
                    Destroy(gameObject);
                }
            } else
            {
                Destroy(gameObject);
                //Debug.Log(currentValue + " Deleteing Current");
            }
        } else
        {
            if (collision.gameObject.name.Contains("FPSPlayer"))
            {
                Debug.Log("You Lost a Life");
            }
            Debug.Log("CurrentValue doesn't match " + currentValue);
        }
        //Debug.Log("Collision with " + collision.gameObject.name);
    }

}