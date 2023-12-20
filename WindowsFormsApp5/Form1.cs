using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpGL;


namespace WindowsFormsApp5
{
    public partial class Form1 : Form
    {
       
        public float x;
        public float y;
        int os_x = 1, os_y = 0, os_z = 0, count = 0; // выбранные оси 
        double a = 0, b = 0, c = -6, d = 0, zoom = 1, flag=0;
        myReadObj obj = new myReadObj();
        
     
        private void timer1_Tick(object sender, EventArgs e)
        {
            
        }


        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            b = (double)trackBar2.Value/1000 ;
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            c = (double)trackBar3.Value / 1000;
        }

        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            d = (double)trackBar4.Value;
        }

        private void trackBar5_Scroll(object sender, EventArgs e)
        {
            zoom = (double)trackBar5.Value/1000;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    {
                        os_x = 1; os_y = 0; os_z = 0;
                        break;
                    }
                case 1:
                    {
                        os_x = 0; os_y = 1; os_z = 0;
                        break;
                    }
                case 2:
                    {
                        os_x = 0; os_y = 0; os_z = 1;
                        break;
                    }
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        class myReadObj
        {
            
            public class POINT3
            {
                public double X;
                public double Y;
                public double Z;
            };
            public class WenLi
            {
                public double TU;
                public double TV;
            };
            public class FaXiangLiang
            {
                public double NX;
                public double NY;
                public double NZ;
            };
            public class Mian
            {
                public int[] V = new int[3];
                public int[] T = new int[3];
                public int[] N = new int[3];
            };
            public class Model
            {

                public List<POINT3> V = new List<POINT3>();
                public List<WenLi> VT = new List<WenLi>();
                public List<FaXiangLiang> VN = new List<FaXiangLiang>();
                public List<Mian> F = new List<Mian>();
                                                       
            }

            public Model mesh = new Model();
            
            
            public static float scale;
           
            public uint showFaceList;

            public int YU = 1;

            public void loadFile(String fileName)
            {
                // Mian[] f;
                //POINT3[] v;
                //FaXiangLiang[] vn;
                //WenLi[] vt;

                StreamReader objReader = new StreamReader(fileName);
                ArrayList al = new ArrayList();
                string texLineTem = "";
                while (objReader.Peek() != -1)
                {
                    texLineTem = objReader.ReadLine();
                    if (texLineTem.Length < 2) continue;
                    if (texLineTem.IndexOf("v") == 0)
                    {
                        if (texLineTem.IndexOf("t") == 1)//Vt
                        {
                            string[] tempArray = texLineTem.Split(' ');
                            WenLi vt = new WenLi();
                            tempArray[1] = tempArray[1].Replace(".", ",");
                            tempArray[2] = tempArray[2].Replace(".", ",");
                            vt.TU = double.Parse(tempArray[1]);
                            vt.TV = double.Parse(tempArray[2]);
                            mesh.VT.Add(vt);
                        }
                        else if (texLineTem.IndexOf("n") == 1)//VN 
                        {
                            string[] tempArray = texLineTem.Split(new char[] { '/', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                            FaXiangLiang vn = new FaXiangLiang();
                            tempArray[1] = tempArray[1].Replace(".", ",");
                            tempArray[2] = tempArray[2].Replace(".", ",");
                            tempArray[3] = tempArray[3].Replace(".", ",");
                            vn.NX = double.Parse(tempArray[1]);
                            vn.NY = double.Parse(tempArray[2]);

                            if (tempArray[3] == "\\")
                            {
                                texLineTem = objReader.ReadLine();
                                vn.NZ = double.Parse(texLineTem);
                            }
                            else vn.NZ = double.Parse(tempArray[3]);

                            mesh.VN.Add(vn);
                        }
                        else
                        {//V
                            string[] tempArray = texLineTem.Split(' ');
                            POINT3 v = new POINT3();
                            System.Console.WriteLine(tempArray[1]);
                            tempArray[1] = tempArray[1].Replace(".", ",");
                            tempArray[2] = tempArray[2].Replace(".", ",");
                            tempArray[3] = tempArray[3].Replace(".", ",");

                            if (string.IsNullOrEmpty(tempArray[1]) == true)
                            {
                                tempArray[4] = tempArray[4].Replace(".", ",");
                                v.X = double.Parse(tempArray[2]);
                                v.Y = double.Parse(tempArray[3]);
                                v.Z = double.Parse(tempArray[4]);
                            }
                            else
                            {
                                v.X = double.Parse(tempArray[1]);
                                v.Y = double.Parse(tempArray[2]);
                                v.Z = double.Parse(tempArray[3]);
                            }
                            
                            mesh.V.Add(v);
                        }
                    }
                    else if (texLineTem.IndexOf("f") == 0)
                    {
                        //f 
                        string[] tempArray = texLineTem.Split(new char[] { '/', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                        Mian f = new Mian();
                        int i = 0;
                        int k = 1;
                        while (i < 3)
                        {
                            if (mesh.V.Count() != 0)
                            {
                                f.V[i] = int.Parse(tempArray[k]) - 1;
                                k++;
                            }
                            if (mesh.VT.Count() != 0)
                            {
                                f.T[i] = int.Parse(tempArray[k]) - 1;
                                k++;
                            }
                            if (mesh.VN.Count() != 0)
                            {
                                f.N[i] = int.Parse(tempArray[k]) - 1;
                                k++;
                            }
                            i++;
                        }
                        mesh.F.Add(f);

                    }
                }


            }

            public uint createListFace(ref SharpGL.OpenGL gl)
            {
                int i = 0;
                 gl.NewList(showFaceList, OpenGL.GL_COMPILE);
                for (i = 0; i < mesh.F.Count(); i++)
                    
                {
                 
                    gl.Begin(OpenGL.GL_TRIANGLES);                          //  Рисуем полигоны 
                    if (mesh.VT.Count() != 0) gl.TexCoord(mesh.VT[mesh.F[i].T[0]].TU, mesh.VT[mesh.F[i].T[0]].TV);  //текстура
                    if (mesh.VN.Count() != 0) gl.Normal(mesh.VN[mesh.F[i].N[0]].NX, mesh.VN[mesh.F[i].N[0]].NY, mesh.VN[mesh.F[i].N[0]].NZ);
                    gl.Vertex(mesh.V[mesh.F[i].V[0]].X / YU, mesh.V[mesh.F[i].V[0]].Y / YU, mesh.V[mesh.F[i].V[0]].Z / YU);        //  вверх
                    if (mesh.VT.Count() != 0) gl.TexCoord(mesh.VT[mesh.F[i].T[1]].TU, mesh.VT[mesh.F[i].T[1]].TV);  //текстура
                    if (mesh.VN.Count() != 0)gl.Normal(mesh.VN[mesh.F[i].N[1]].NX, mesh.VN[mesh.F[i].N[1]].NY, mesh.VN[mesh.F[i].N[1]].NZ);
                    gl.Vertex(mesh.V[mesh.F[i].V[1]].X / YU, mesh.V[mesh.F[i].V[1]].Y / YU, mesh.V[mesh.F[i].V[1]].Z / YU);        //  нижний левый
                    if (mesh.VT.Count() != 0) gl.TexCoord(mesh.VT[mesh.F[i].T[2]].TU, mesh.VT[mesh.F[i].T[2]].TV);  //текстура
                    if (mesh.VN.Count() != 0)gl.Normal(mesh.VN[mesh.F[i].N[2]].NX, mesh.VN[mesh.F[i].N[2]].NY, mesh.VN[mesh.F[i].N[2]].NZ);
                    gl.Vertex(mesh.V[mesh.F[i].V[2]].X / YU, mesh.V[mesh.F[i].V[2]].Y / YU, mesh.V[mesh.F[i].V[2]].Z / YU);        //  нижний правый



                    gl.End();
                    

                   
                }
                gl.EndList();
                return showFaceList;

            }
            


        }
        private void openGLControl1_OpenGLDraw(object sender, SharpGL.RenderEventArgs args)
        {
            

        }

        float[] glfMatAmbient = { 0.200f, 0.200f, 0.200f, 1.0f };
        float[] glfMatDiffuse = { 0.200f, 0.200f, 0.200f, 1.0f };
        float[] glfMatSpecular = { 1.000f, 1.000f, 1.000f, 1.0f };
        float[] glfMatEmission = { 0.000f, 0.000f, 0.000f, 1.0f };
        float fShininess = 50.000f;

        private void Form1_Load(object sender, EventArgs e)
        {
            
          
        }

        private void openGLControl1_OpenGLDraw_1(object sender, SharpGL.RenderEventArgs args)
        {

            SharpGL.OpenGL gl = this.openGLControl1.OpenGL;
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity();

            float[] glfLight = { -4.0f, 4.0f, 4.0f, 0.0f };
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, glfLight);

           
            gl.Translate(a, b, c);

            // поворот
            gl.Rotate(d, os_x, os_y, os_z);
            // масштабирование
            gl.Scale(zoom, zoom, zoom);
    
            obj.createListFace(ref gl);
            gl.CallList(obj.showFaceList);

           
        }

        private void openGLControl1_OpenGLInitialized(object sender, EventArgs e)
        {
            SharpGL.OpenGL gl = this.openGLControl1.OpenGL;
      
            gl.Enable(OpenGL.GL_LIGHT0);
            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_DEPTH_TEST);
          

            gl.Material(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT, glfMatAmbient);
            gl.Material(OpenGL.GL_FRONT, OpenGL.GL_DIFFUSE, glfMatDiffuse);
            //gl.Material(OpenGL.GL_FRONT, OpenGL.GL_SPECULAR, glfMatSpecular);
            gl.Material(OpenGL.GL_FRONT, OpenGL.GL_EMISSION, glfMatEmission);
            gl.Material(OpenGL.GL_FRONT, OpenGL.GL_SHININESS, fShininess);

            obj.loadFile("cube.obj");
            comboBox1.SelectedIndex = 0;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            a = (double)trackBar1.Value/1000 ;
        }
    }

}
