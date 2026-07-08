using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MothercareImportData.Models
{
    public class ItemMasterRecord
    {
        public string Code { get; set; } //Κωδικός είδους
        public string Name { get; set; } //Περιγραφη
        public string TaxCode { get; set; } //Φορολογικός κωδικός
        public string AssortmentDescription { get; set; } //Περιγραφή assortment
        public string SupplierCode { get; set; } //Κωδικός προμηθευτή
        public string EnglishDescription { get; set; } //Αγγλικη περιγραφή
        public string Comments { get; set; } //Σχόλια
        public string UnitOfMeasure { get; set; } //Μονάδα μέτρησης
        public int PackageQuantity { get; set; } //Τεμάχια συσκευασίας
        public int ComposedOfQuantity { get; set; } //Από πόσα τεμάχια αποτελείται
        public int Division { get; set; } //Division
        public int Department { get; set; } //Department
        public int Subdept { get; set; } //Subdept
        public int Class { get; set; } //Class
        public int Year { get; set; } //Έτος
        public int Season { get; set; } //Εποχή
        public int StatisticalYear { get; set; } //Στατιστικό έτος
        public string StyleNo { get; set; } //Style No
        public string Size { get; set; } //Size
        public string Color { get; set; } //Color
        public int Brand { get; set; } //Brand
        public int House { get; set; } //Οίκος
        public int VatCategory { get; set; } //Vat category
        public int Phase { get; set; } //Φάση
        public string Seasonality { get; set; } //Εποχικότητα
        public int ListUp { get; set; } //ListUp
        public int Outlet { get; set; } //OUTLET
        public float NetWeight { get; set; } //Καθαρό βάρος
        public string CountryOfOrigin { get; set; } //COO
        public string Intrastat { get; set; } //Intrastat
        public int Status { get; set; } //Status
        public string Collection { get; set; } //Συλλογή
        public string CommercialCollection { get; set; } //Εμπ. συλλογή
        public string Bu { get; set; } //BU
        public int ItemType { get; set; } //Τύπος Είδους
        public string AccountingType { get; set; } //Τύπος για λογιστική
        public string ImagePath { get; set; } //Image path
        public int RestockWithPackage { get; set; } //Ανεφοδιασμός με συσκευασία
        public int WarrantyMonths { get; set; } //μήνες εγγύησης
        public string EshopMasterCode { get; set; } //Κωδ. master για e-shop
        public float Height { get; set; } //HEIGHT
        public float Length { get; set; } //LENGTH
        public float Width { get; set; } //WIDTH
        public float ItemCubeM { get; set; } //ITEMCUBEM
        public string PhotoName { get; set; } //Όνομα φωτογραφίας
        public int WorkInProgressInGr { get; set; } //WORK_IN_PROGRESS_IN_GR
        public int ToBePublishedInGr { get; set; } //TO_BE_PUBLISHED_IN_GR
        public int ToBeUnpublishedInGr { get; set; } //TO_BE_UNPUBLISHED_IN_GR
        public int HasTranslation { get; set; } //HAS_TRANSLATION
        public int IsPublishedInGr { get; set; } //IS_PUBLISHED_IN_GR
        public int ToBePublishedInSkroutz { get; set; } //TO_BE_PUBLISHED_IN_SKROUTZ
    }
}
