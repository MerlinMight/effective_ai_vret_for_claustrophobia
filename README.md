# AI-based Virtual Reality Exposure Therapy (VRET) for Claustrophobia

## 📌 Overview
Claustrophobia is an anxiety disorder characterized by an intense fear of enclosed spaces. Traditional exposure therapy can be difficult due to accessibility limitations, discomfort, and lack of personalization. This project presents an **AI-baseded Virtual Reality Exposure Therapy (VRET) system** designed to help individuals gradually confront claustrophobic environments in a safe, controlled, and adaptive virtual setting.

The system combines **AI-generated immersive environments**, **real-time stress monitoring**, and **interactive user controls** to deliver a personalized therapeutic experience.

---

## 🎯 Objectives
- Simulate realistic confined spaces using AI-generated VR environments
- Provide gradual exposure through increasing difficulty levels
- Ensure user safety using real-time physiological and behavioral monitoring
- Allow users to control therapy flow and relaxation
- Automatically generate session-wise progress reports

---

## 🧠 System Features

### 1. AI-Generated Virtual Environments
- Uses the **Skybox API** to generate high-quality 360° environments
- Simulates confined spaces such as:
  - Small rooms
  - Elevators
  - Tunnels
- Environments progress from less intense to highly claustrophobic scenarios

---

### 2. Interactive Therapy Controls
The VR environment includes the following controls:
- **Next Level Button** – moves to a more challenging environment
- **Loop Button** – repeats the current level with slight variations
- **Relaxation Button** – switches to a calming environment instantly
- **End Session Button** – ends the session and generates a report

These controls allow users to proceed at their own pace and feel in control of the therapy.

---

## ❤️ Adaptive Stress Monitoring

### Heart Rate Monitoring
- Receives real-time heart rate data from an external wearable device (e.g., fitness band)
- Uses **UDP communication** to send data to the Unity application
- If heart rate remains above a predefined threshold:
  - The system automatically switches to a calming environment
- Therapy resumes once heart rate stabilizes

---

### Stress Monitoring via Movement
- Continuously tracks head and body movement
- Rapid or frequent movements are treated as signs of anxiety
- Sustained movement triggers an automatic transition to a relaxation scene
- Therapy resumes once movement patterns return to normal

This dual-sensing approach enhances user safety and personalization.

---

## 📊 Automatic Session Report Generation
At the end of each session, a personalized report is generated automatically using the **End Session button**.

### Report Includes:
- Participant identification
- Date and time of the session
- Time spent in each stage
- Panic / relaxation button usage
- Observations during the session

These reports help users and therapists track progress, identify stress triggers, and plan future sessions effectively.

---

## 🏗️ System Architecture (High-Level)
- **VR Engine:** Unity
- **Environment Generation:** Skybox API
- **Stress Detection:** Heart rate monitoring + movement analysis
- **Backend Logic:** Real-time adaptive scene control
- **Hardware:** Compatible with most VR head-mounted displays (HMDs)

---

## 🛠️ Technologies Used
- Unity (C#)
- Skybox API
- Virtual Reality (VR)
- UDP Communication
- Wearable Heart Rate Sensors
- VR Head-Mounted Displays (HMDs)

---

## 🚀 Use Cases
- Exposure therapy for claustrophobia
- Mental health research and studies
- AI-driven adaptive therapy systems
- VR-based psychological training

---

## 🔮 Future Enhancements
- Therapist dashboard for remote monitoring
- Machine learning-based stress prediction
- Multi-session progress analytics
- Support for additional phobias
- Cloud-based report storage

---
### Note
This project is intended for academic purposes.  
Due to dependencies on VR hardware and paid APIs, full execution may require additional configuration.

---
