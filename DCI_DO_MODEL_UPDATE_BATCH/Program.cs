using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DCI_DO_MODEL_UPDATE_BATCH
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int milliseconds = 100;
            SqlConnectDB SCM = new SqlConnectDB("dbSCM");

            List<Models> LIST_DO_PART_MASTER = new List<Models>();
            SqlCommand STR_GET_DO_PART = new SqlCommand();
            STR_GET_DO_PART.CommandText = @"SELECT  PART_ID,PARTNO,CM,VD_CODE,DESCRIPTION,UNIT FROM [dbSCM].[dbo].[DO_PART_MASTER]";
            DataTable dtDOPARTMaster = SCM.Query(STR_GET_DO_PART);
            foreach (DataRow drPartMaster in dtDOPARTMaster.Rows)
            {
                Models ITEM = new Models();
                ITEM.PART_ID = Convert.ToInt32(drPartMaster["PART_ID"].ToString());
                ITEM.PARTNO = drPartMaster["PARTNO"].ToString();
                ITEM.CM = drPartMaster["CM"].ToString();
                ITEM.DESCRIPTION = drPartMaster["DESCRIPTION"].ToString();
                ITEM.VD_CODE = drPartMaster["VD_CODE"].ToString();
                ITEM.UNIT = drPartMaster["UNIT"].ToString();
                LIST_DO_PART_MASTER.Add(ITEM);
            }


            SqlCommand STR_AL_PART = new SqlCommand();
            STR_AL_PART.CommandText = @"SELECT DrawingNo as PARTNO,CM,(Description) as DESCRIPTION ,VenderCode as VD_CODE,IVUnit AS UNIT FROM [dbSCM].[dbo].[AL_Part] where Route = 'D' and VenderCode != ''";
            DataTable dtALPart = SCM.Query(STR_AL_PART);
            foreach (DataRow dr in dtALPart.Rows)
            {
                string PARTNO = dr["PARTNO"].ToString();
                string CM = dr["CM"].ToString();
                string DESCRIPTION = dr["DESCRIPTION"].ToString();
                string VD_CODE = dr["VD_CODE"].ToString();
                string UNIT = dr["UNIT"].ToString();
                Models ITEM_DO_PART = LIST_DO_PART_MASTER.FirstOrDefault(x => x.PARTNO == PARTNO);
                if (ITEM_DO_PART != null)
                {
                    if (ITEM_DO_PART.CM != CM || ITEM_DO_PART.VD_CODE != VD_CODE || ITEM_DO_PART.DESCRIPTION != DESCRIPTION || ITEM_DO_PART.UNIT != UNIT)
                    {
                        // UPDATE CM or VD or DESC or UNIT  
                        SqlCommand sqlUpdate = new SqlCommand();
                        sqlUpdate.CommandText = @"UPDATE [dbSCM].[dbo].[DO_PART_MASTER] SET CM = @CM , VD_CODE = @VD_CODE , UNIT = @UNIT, DESCRIPTION = @DESC,UPDATE_DATE = GETDATE(),UPDATE_BY = 'BATCH' WHERE PARTNO = @PARTNO";
                        sqlUpdate.Parameters.Add(new SqlParameter("@CM", CM));
                        sqlUpdate.Parameters.Add(new SqlParameter("@VD_CODE", VD_CODE));
                        sqlUpdate.Parameters.Add(new SqlParameter("@UNIT", UNIT));
                        sqlUpdate.Parameters.Add(new SqlParameter("@DESC", DESCRIPTION));
                        sqlUpdate.Parameters.Add(new SqlParameter("@PARTNO", PARTNO));
                        int update = SCM.ExecuteNonCommand(sqlUpdate);
                        if (update > 0)
                        {
                            Console.WriteLine("---- UPDATE SUCCESS PART : " + PARTNO + " CM : " + CM + " VD_CODE : " + VD_CODE + "  ----");
                        }
                        else
                        {
                            Console.WriteLine("---- UPDATE FALSE PART : " + PARTNO + " ----");
                        }
                    }
                }
                else
                {
                    SqlCommand sqlInsert = new SqlCommand();
                    sqlInsert.CommandText = @"INSERT INTO [dbSCM].[dbo].[DO_PART_MASTER]  ([PARTNO] ,[CM] ,[VD_CODE] ,[DESCRIPTION] ,[PDLT] ,[UNIT] ,[BOX_MIN] ,[BOX_MAX] ,[BOX_QTY] ,[UPDATE_DATE] ,[UPDATE_BY]) VALUES (@PARTNO,@CM,@VD_CODE,@DESCRIPTION,0,@UNIT,0,99999,0,GETDATE(),'BATCH')";
                    sqlInsert.Parameters.Add(new SqlParameter("@PARTNO", PARTNO));
                    sqlInsert.Parameters.Add(new SqlParameter("@CM", CM));
                    sqlInsert.Parameters.Add(new SqlParameter("@VD_CODE", VD_CODE));
                    sqlInsert.Parameters.Add(new SqlParameter("@DESCRIPTION", DESCRIPTION));
                    sqlInsert.Parameters.Add(new SqlParameter("@UNIT", UNIT));
                    int insert = SCM.ExecuteNonCommand(sqlInsert);
                    if (insert > 0)
                    {
                        Console.WriteLine("---- INSERT [SUCCESS] PART : " + PARTNO + " CM : " + CM + " VD_CODE : " + VD_CODE + "  ----");
                    }
                    else
                    {
                        Console.WriteLine("---- INSERT [FALSE] PART : " + PARTNO + " CM : " + CM + " VD_CODE : " + VD_CODE + "  ----");
                    }
                }

            }
        }
    }
}
