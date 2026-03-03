import json
import random
import csv

doms = ['frontend', 'backend', 'fullstack', 'data', 'devops', 'ml', 'business', 'design', 'healthcare', 'education']

# We will procedurally generate 150 questions by combining contexts and actions.
contexts = [
    "in a team setting", "when working independently", "during a crisis",
    "when exploring new concepts", "in a leadership role", "when helping a friend",
    "at a hackathon", "during a weekend project", "in a corporate environment",
    "when starting a new job", "in a high-pressure situation", "during casual networking",
    "when evaluating a new tool", "while reading the news", "in a creative workshop"
]

actions = [
    "what do you prioritize?", "what sounds most appealing?", "where do you focus your energy?",
    "what is your first instinct?", "how do you contribute?", "what tool do you reach for?",
    "what outcome do you hope for?", "what problem do you tackle first?", "what role do you take on?", "how do you organize the work?"
]

option_templates = {
    'frontend': ["polishing the UI", "creating interactive components", "focusing on user experience", "optimizing browser performance", "writing React components", "setting up CSS layouts", "ensuring accessibility", "adding animations", "building mobile-friendly views"],
    'backend': ["designing database schemas", "writing robust APIs", "optimizing server response times", "implementing secure authentication", "writing C# logic", "setting up REST endpoints", "handling data persistence", "managing application state", "containerizing microservices"],
    'fullstack': ["connecting the UI to the database", "handling both server logic and client views", "architecting the entire application flow", "building end-to-end features", "working across the stack", "integrating the API with the frontend", "deploying a full web app", "managing the system architecture", "rapidly prototyping the whole product"],
    'data': ["analyzing hidden trends", "cleaning and querying large datasets", "building statistical models", "finding actionable business insights", "writing complex SQL scripts", "using Python Pandas", "creating data visualizations", "ensuring data integrity", "predicting future trends"],
    'devops': ["automating deployment pipelines", "monitoring server health", "managing cloud infrastructure", "configuring CI/CD", "writing bash scripts", "setting up AWS/Azure resources", "ensuring zero downtime", "orchestrating containers with Kubernetes", "securing the network"],
    'ml': ["training predictive models", "tuning neural networks", "designing AI algorithms", "implementing deep learning", "processing natural language", "building computer vision tools", "optimizing model accuracy", "researching AI papers", "evaluating model biases"],
    'business': ["planning the financial budget", "leading meetings and strategy", "analyzing market competition", "managing project timelines", "negotiating with clients", "driving revenue growth", "optimizing operational costs", "creating a pitch deck", "evaluating business risk"],
    'design': ["sketching wireframes", "choosing color palettes", "illustrating graphics", "designing brand logos", "prototyping in Figma", "running user research sessions", "crafting visual storytelling", "focusing on esthetics", "creating typography systems"],
    'healthcare': ["diagnosing patient symptoms", "researching medical treatments", "providing clinical care", "studying human anatomy", "monitoring vital signs", "helping people recover", "organizing medical records", "assisting in procedures", "promoting wellness"],
    'education': ["creating lesson plans", "teaching and mentoring students", "explaining complex concepts", "grading assignments", "researching pedagogical methods", "organizing classroom activities", "evaluating student progress", "designing curriculum", "inspiring others to learn"]
}

questions = []
q_id = 1

for c in contexts:
    for a in actions:
        if q_id > 150:
            break
            
        topic_text = f"Imagine you are {c}. {a.capitalize()}"
        
        # Pick 4 random domains to be the options for this question
        chosen_domains = random.sample(doms, 4)
        options = []
        for i, d in enumerate(chosen_domains):
            opt_text = random.choice(option_templates[d])
            options.append({
                "id": f"q{q_id}_opt{i+1}",
                "text": opt_text.capitalize(),
                "domain": d
            })
            
        questions.append({
            "id": f"q{q_id}",
            "text": topic_text,
            "category": "General",
            "options": options
        })
        q_id += 1

# Save questions to JSON
with open('quiz_150_questions.json', 'w') as f:
    json.dump(questions, f, indent=2)

print(f"Generated {len(questions)} distinct questions.")

# Generate synthetic training data
# We'll simulate 5000 users. Each user will get 10 random questions from the 150 pool.
# Since ML models usually need fixed feature inputs, and a user only answers 10/150 questions,
# we need a unified feature space. The easiest way is that the features are the COUNT 
# of times the user selected each domain.
# Since there are 10 domains, the features are: frontend_count, backend_count...
# Wait, if we use those as features, it's just a simple max function.
# To make it a proper ML problem that learns weights (e.g., maybe 'building ML models' is highly predictive of ML, but 'analyzing data' is data),
# we should make the features the ACTUAL options selected.
# If we have 150 questions, each with 4 options, that's 600 possible categorical features.
# A user answers 10 questions, so 10 features are 1, 590 are 0.
# We will create a dataset where columns are q1_opt1, q1_opt2, ... q150_opt4, and Label is domain.

# Wait, a simpler ML approach:
# Just output the 10 selected question IDs and Option IDs, but for standard ML.NET it's easier to use text classification
# or a vector. Let's output a sparse vector or just a string of the text they chose!
# If we concatenate the text of the 10 chosen options into one block of text, we can use ML.NET Text Classification!
# This is a very robust way to handle variable arrays of questions.

dataset = []
for _ in range(5000):
    # User has a true underlying domain
    user_true_domain = random.choice(doms)
    
    # User is asked 10 random questions
    asked_questions = random.sample(questions, 10)
    
    chosen_texts = []
    
    for q in asked_questions:
        # User has an 80% chance of picking the option that matches their true domain (if available)
        # otherwise picks randomly
        matching_opts = [o for o in q['options'] if o['domain'] == user_true_domain]
        if matching_opts and random.random() < 0.8:
            selected = random.choice(matching_opts)
        elif matching_opts and random.random() < 0.9:
            # Pick a related domain 
            selected = random.choice(q['options'])
        else:
            selected = random.choice(q['options'])
            
        chosen_texts.append(selected['text'])
        
    combined_text = " ".join(chosen_texts)
    dataset.append([combined_text, user_true_domain])

# Write to CSV
with open('quiz_training_data.csv', 'w', newline='', encoding='utf-8') as f:
    writer = csv.writer(f)
    writer.writerow(["CombinedAnswers", "RecommendedDomain"])
    writer.writerows(dataset)

print(f"Generated 5000 synthetic responses in quiz_training_data.csv.")
